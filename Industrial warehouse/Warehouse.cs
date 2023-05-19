using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Industrial_warehouse
{
    internal class Warehouse
    {
        [JsonProperty]
        SortedDictionary<double,SortedDictionary<double, BoxPile>> BoxCollection; // the main data structure, where boxes are stored according to width and height
        [JsonProperty]
        SortedSet<BoxPile> BoxTimeCollection;  // seconday data structure, where boxes are stored according to activity
         
        public Warehouse()
        {
            BoxCollection = new SortedDictionary<double, SortedDictionary<double, BoxPile>>();
            BoxTimeCollection = new SortedSet<BoxPile>(new BoxPileByDateComparer());
        }
        public BoxPile GetBoxPile(double x, double y)
        {
            /// <summary>
           // returns all boxes of a given size, or null if they do not exit, (the function does not delete from database)
            /// </summary>
            SortedDictionary<double, BoxPile> SubCollectionByY;
            BoxPile boxPile;
            if (!BoxCollection.TryGetValue(x, out SubCollectionByY) || !SubCollectionByY.TryGetValue(y,out boxPile))
            {
                return null;
            }
            return boxPile;
        } 
        public void AddBoxPile(BoxPile boxPile)
        {
            /// <summary>
            // add boxPile object to the warehouse if it already exists the amount field is increasd
            /// </summary>
            if (boxPile.Amount > Configurations.MaxSameSizeBoxes)
            { throw new ArgumentException($"Cannot add this many boxes of this size, as the max capacity is {Configurations.MaxSameSizeBoxes} for piles of a given size"); }
            if(BoxCollection.ContainsKey(boxPile.X))
            {
                if (BoxCollection[boxPile.X].ContainsKey(boxPile.Y))
                {
                    BoxPile foundBox = BoxCollection[boxPile.X][boxPile.Y];
                    if(foundBox.Amount + boxPile.Amount > Configurations.MaxSameSizeBoxes)
                    { throw new ArgumentException($"Cannot add this many boxes of this size, as there are already {foundBox.Amount} and the max capacity is {Configurations.MaxSameSizeBoxes}"); }
                    foundBox.Amount += boxPile.Amount;
                }
                else
                {
                    BoxCollection[boxPile.X].Add(boxPile.Y, boxPile);
                    BoxTimeCollection.Add(boxPile);
                }
            }
            else
            {
                BoxCollection.Add(boxPile.X, new SortedDictionary<double, BoxPile>());
                BoxCollection[boxPile.X].Add(boxPile.Y, boxPile);
                BoxTimeCollection.Add(boxPile);
            }
        }
              
        public BoxPile GetBox(double x, double y)
        {
            /// <summary>
            // removes a single box from the structures and returns it, if no box is of appropreate size (req +config deviation) return null
            /// </summary>
            BoxPile foundBoxPile = null;
            if (BoxCollection.ContainsKey(x) && BoxCollection[x].ContainsKey(y))
            { foundBoxPile = BoxCollection[x][y]; }
            else if(BoxCollection.ContainsKey(x))
            {             
                foundBoxPile = FindBoxPileInY(BoxCollection[x], y);
                if(foundBoxPile == null)
                { foundBoxPile = FindBoxPileInX(x, y); }
            }
            else
            { foundBoxPile = FindBoxPileInX(x, y); }
            
            if(foundBoxPile == null)
            { return null; }

            foundBoxPile.Amount--;
            BoxTimeCollection.Remove(foundBoxPile);
            if(foundBoxPile.Amount == 0)
            {
                DeleteBoxPile(foundBoxPile);
            }
            else
            {
                foundBoxPile.LastActivityDate = DateTime.Now;
                BoxTimeCollection.Add(foundBoxPile);
            }
            return new BoxPile (foundBoxPile.X,foundBoxPile.Y,1,foundBoxPile.LastActivityDate);
        }

        

        public List<BoxPile> GetBoxes(double x, double y, int amount = 1, int NumOfLoop = 1)
        {
            /// <summary>
            // "GetBoxes" removes multipule boxes and returns them as a list. the list will not exeed the length defined in the configuration class (under MaxDiffrentSizes)
            // this function is recursive, with each iteration the right boxed are removed, untill there are no boxes of sutable size (according to input and the max deviation in config class)
            // or untill the maximal number of diffrent sizes is reached  (again under MaxDiffrentSizes)
            /// </summary>
            List<BoxPile> FoundBoxes = new List<BoxPile>();
            if(NumOfLoop > Configurations.MaxDiffrentSizes)
            { return FoundBoxes; }  
            if(BoxCollection.ContainsKey(x))
            {
                SortedDictionary<double,BoxPile> innerByY = BoxCollection[x];
                if (innerByY.ContainsKey(y))
                {
                    BoxPile found = innerByY[y];
                    AddBoxToList(ref FoundBoxes, found, x, y, amount,NumOfLoop+1);
                }
                else
                {
                    BoxPile found = FindBoxPileInY(innerByY, y);
                    if(found != null)
                    {
                        AddBoxToList(ref FoundBoxes, found, x, y, amount, NumOfLoop + 1);
                    }
                    else
                    {
                        found = FindBoxPileInX(x, y);
                        if(found!=null)
                        {
                            AddBoxToList(ref FoundBoxes, found, x, y, amount, NumOfLoop + 1);
                        }
                    }
                }
            }
            else
            {
                BoxPile found = FindBoxPileInX(x, y);
                if (found != null)
                {
                    AddBoxToList(ref FoundBoxes, found, x, y, amount, NumOfLoop + 1);
                }
            }
            return FoundBoxes;

        }                                              
        private void AddBoxToList(ref List<BoxPile> foundBoxes, BoxPile found, double x, double y, int amount, int NumOfLoop = 1)
        {
            /// <summary>
            // helper function to the GetBoxes function, to avoid writing the same code multipule times. in it called from the main function, and in return it calls the main function as part of the recursion process
            /// </summary>
            if (found.Amount >= amount)
            {
                found.Amount -= amount;
                if (found.Amount == 0)
                { DeleteBoxPile(found); }                
                foundBoxes.Add(new BoxPile(found.X, found.Y, amount, found.LastActivityDate));
            }
            else
            {
                DeleteBoxPile(found);
                foundBoxes = GetBoxes(x, y, amount - found.Amount, NumOfLoop);
                foundBoxes.Add(found);               
            }
        }

        public void RemoveInactiveBoxes(out List<string> info)
        {
            /// <summary>
            // removes boxes with last acitiviy date smaller then datetime.now - timeSpan defined in configurations (as MaxInactivityPeriod)
            /// </summary>
            info = new List<string>();
            BoxPile[] InactiveBoxes = BoxTimeCollection.Where((pb)=>(DateTime.Now)-pb.LastActivityDate > Configurations.MaxInactivityPeriod).ToArray();
            for(int i=0;i < InactiveBoxes.Length;i++)
            {
                DeleteBoxPile(InactiveBoxes[i]);
                info.Add($" Boxes of size {InactiveBoxes[i].X},{InactiveBoxes[i].Y} were removed (last activity: {InactiveBoxes[i].LastActivityDate})");
            }
        }

        public List<BoxPile> GetInactiveBoxesBySpan(TimeSpan timeSpan)
        {
          return BoxTimeCollection.GetViewBetween(new BoxPile(1, 1, 1, DateTime.MinValue),new BoxPile(1, 1, 1, DateTime.Now - timeSpan)).ToList();
        }

        public void DeleteBoxPile(BoxPile boxPile)
        {
            BoxCollection[boxPile.X].Remove (boxPile.Y);
            if (!BoxCollection[boxPile.X].Any())
            {
                BoxCollection.Remove(boxPile.X);
            }
            BoxTimeCollection.Remove(boxPile);
        }

        private BoxPile FindBoxPileInX(double x, double y)
        {
            /// <summary>
            // returns the first box with apropreate width and length, (larger then rq but deviation limited according to configuration class)
            // if no box is found, return null
            /// </summary>
            BoxPile foundBoxPile = null;
            IEnumerable<KeyValuePair<double, SortedDictionary<double, BoxPile>>> PossibleInnerDicts = BoxCollection.Where((Inner) => Inner.Key > x && Inner.Key <= x * (Configurations.MaxSizeDeviation + 1));
            if (!PossibleInnerDicts.Any())
            {
                foundBoxPile = null;
            }
            else
            {
                foreach (KeyValuePair<double, SortedDictionary<double, BoxPile>> InnerDictPair in PossibleInnerDicts)
                {
                    foundBoxPile = FindBoxPileInY(InnerDictPair.Value, y);
                    if (foundBoxPile != null)
                    { break; }
                }
            }
            return foundBoxPile;
        }

        public void UpdateActivity(BoxPile boxPile)
        {
            if(BoxCollection.ContainsKey(boxPile.X) && BoxCollection[boxPile.X].ContainsKey(boxPile.Y))
            {
                BoxPile found = BoxCollection[boxPile.X][boxPile.Y];
                BoxTimeCollection.Remove(found);
                found.LastActivityDate = DateTime.Now;
                BoxTimeCollection.Add(found);
            }
        }

        private BoxPile FindBoxPileInY(SortedDictionary<double, BoxPile> InnerDict, double y)
        {
            /// <summary>
            // returns the first box with apropreate width and length in the given InnerDictionary, (larger then rq but deviation limited according to configuration class)
            // if no box is found, return null
            /// </summary>
            if (InnerDict.ContainsKey(y))
            { return InnerDict[y]; }
            IEnumerable<KeyValuePair<double, BoxPile>> PossibleBoxes = InnerDict.Where((bp) => (bp.Key <= y * (1 + Configurations.MaxSizeDeviation) && bp.Key > y));
            if(PossibleBoxes.Any())
            { return PossibleBoxes.First().Value; }    
            return null;
        }        
    }
}
