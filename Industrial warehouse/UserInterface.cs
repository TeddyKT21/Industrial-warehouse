using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Industrial_warehouse
{
    internal class UserInterface
    {       
        static Warehouse warehouse;
        static void Main(string[] args)
        {          
            warehouse = DataBaseManager.Load();
            RemoveInactiveBoxes();
            while(true)
            {
                Console.WriteLine("Hello ! please select what would you like to do (using the relevant digit)\n");
                Console.WriteLine("1. Remove a single box\n2. Remove multipule  boxes\n3. Add boxes to stock\n4. Get information on a box\n5. Manually remove inactive Box Sizes (Done automatically when booting the program)\n6. Destroy all boxes\n7. Show a list of boxes not sold for a requested period of time\n8. Save current State (otherwise changes will not be saved !)");
                int digit;
                string input = Console.ReadLine();
                if(input == "Exit")
                { break; }
                if(!(int.TryParse(input, out digit) && digit < 9 && digit > 0))
                {
                    Console.WriteLine("invalid input !");
                }                    
                switch(digit)
                {
                    case 1:
                        RemoveOneBox();
                        break;                      
                    case 2:
                        RemoveMultipuleBoxes();
                        break;                       
                    case 3:
                        AddToStock();
                        break;
                    case 4:
                        GetInfo();
                        break;
                    case 5:
                        RemoveInactiveBoxes();
                        break;                   
                    case 6:
                        EmptyWarehouse();
                        break;
                    case 7:
                        BoxesByInactivity();
                        break;
                    case 8:
                        SaveState();
                        break;
                }
                Console.WriteLine("\npress enter to preform another action, or enter 'Exit' to close the program\n");
                
                string str = Console.ReadLine();
                Console.Clear();
                if (input == "Exit")
                { break; }
                   
            }
        }        
        private static void BoxesByInactivity()
        {
            /// <summary>
            /// in order for this function to work (for boxes to exeed the inactivity perioud), change the "MaxInactivityPeriod" to a low enough value , then reboot the program and try again this function
            /// </summary>
            double days;
            Console.WriteLine("(Enter 'Cancel' to go back to menu)");
            Console.WriteLine("Enter the requested inactivity period (in days)");
            string str = Console.ReadLine();
            if(str == "Cancel")
            {
                return;
            }
            if (!double.TryParse(str, out days) || days < 0)
            {
                Console.WriteLine("Invalid number of Days");
                BoxesByInactivity();
            }
            TimeSpan timeSpan = TimeSpan.FromDays(days);            
            List<BoxPile> boxes = warehouse.GetInactiveBoxesBySpan(timeSpan);
            if(boxes.Count == 0)
            {
                Console.WriteLine("No boxes found");
                return;    
            }
            Console.WriteLine($"the following boxes were not bought for {days} days or more");
            foreach(BoxPile box in boxes)
            {
                Console.WriteLine(box);
            }
        }

        private static void EmptyWarehouse()
        {
            warehouse = new Warehouse();
        }

        private static void SaveState()
        {
            DataBaseManager.Save(warehouse);
            Console.WriteLine("Warehouse state saved");
        }

        private static void GetInfo()
        {
            double x;
            double y;
            if (!GetSize(out x, out y))
            {
                return;
            }
            Console.WriteLine(warehouse.GetBoxPile(x, y));
        }
      
        private static void RemoveInactiveBoxes()
        {
            List<string> info;
            warehouse.RemoveInactiveBoxes(out info);
            if(info.Count > 0)
            {
                Console.WriteLine("The following boxes were removed due to inactivity:");
            }
            foreach(string messege in info)
            {
                Console.WriteLine(messege);
            }
        }

        private static void AddToStock()
        {
            double x;
            double y;
            int amount;
            if (! GetSizeAndAmount(out x, out y,out amount))
            {
                return;
            }            
            try
            {
                BoxPile boxPile = new BoxPile(x,y,amount,DateTime.Now);
                warehouse.AddBoxPile(boxPile);
                CheckCriticalAmount(boxPile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                AddToStock();
            }
        }

        private static void RemoveMultipuleBoxes()
        {
            double x;
            double y;
            int amount;
            if(!GetSizeAndAmount(out x, out y, out amount))
            {
                return;
            }
            List<BoxPile> boxes = warehouse.GetBoxes(x,y,amount);
            if(boxes.Count == 0)
            {
                Console.WriteLine("No sutable boxes found");
                return;
            }
            Console.WriteLine("the following boxes were found to match your request:");
            foreach(BoxPile boxPile in boxes)
            {
                Console.WriteLine($"{boxPile.Amount} boxes of size {boxPile.X},{boxPile.Y}");
            }
            Console.WriteLine("do you accept this transaction ? (Y/N)");
            string input = Console.ReadLine();
            while (input != "Y" && input!="N")
            {
                Console.WriteLine("Invalid input !");
                Console.WriteLine("Confirm this transaction (Y/N)");
                input = Console.ReadLine();
            }
            if(input == "N")
            {
                Console.WriteLine("Putting boxes back in storage");
                foreach(BoxPile boxPile in boxes)
                {
                    warehouse.AddBoxPile(boxPile);
                }
            }
            else
            {                
                Console.WriteLine("The boxes are permenently removed");
                foreach (BoxPile boxPile in boxes)
                {
                    warehouse.UpdateActivity(boxPile);
                    CheckCriticalAmount(warehouse.GetBoxPile(boxPile.X,boxPile.Y),boxPile.X,boxPile.Y);                  
                }
            }
        }

        private static bool GetSizeAndAmount(out double x, out double y, out int amount)
        {
            amount = 0;            
            if(!GetSize(out x, out y))
            {
                return false;
            }
            Console.WriteLine("How many boxes ?");
            string str = Console.ReadLine();
            if (str == "Cancel")
            {
                return false;
            }
            if(!int.TryParse(str, out amount) || amount <=0)
            {
                Console.WriteLine("Invalid amount !");
                GetSizeAndAmount(out x,out y, out amount);
            }
            return true;
        }

        private static void RemoveOneBox()
        {
            double x;
            double y;
            if(!GetSize(out x, out y))
            {
                return;
            }
           BoxPile rightBox = warehouse.GetBox(x, y);
            if (rightBox == null)
            {
                Console.WriteLine("No box with the right dimentions exists");
            }
            else
            {
                Console.WriteLine($"Found Box! with a width of: {rightBox.X}, and a height of: {rightBox.Y}");
            }
            CheckCriticalAmount(warehouse.GetBoxPile(x, y), rightBox.X,rightBox.Y);

        }

        private static void CheckCriticalAmount(BoxPile InCheck, double x = 0, double y = 0)
        {
            if(InCheck == null)
            {
                Console.WriteLine($"The warehouse ran out of boxes of size {x}, {y}");
                return;
            }
            if(InCheck.Amount <=Configurations.CriticalValSameSizeBoxes)
            {
                Console.WriteLine($"Number of boxes of size {x}, {y} critically low, only {InCheck.Amount} left");
            }
        }

        private static bool GetSize(out double x, out double y)
        {
            while(true)
            {
                Console.WriteLine("(enter 'Cancel' to go back to menu)");
                Console.WriteLine("enter the width of the box");
                string str = Console.ReadLine();
                if(str == "Cancel")
                {
                    x = 0;
                    y = 0;
                    return false; 
                }
                
                if(!double.TryParse(str,out x))
                {
                    Console.WriteLine("Invalid width !");
                    continue;
                }
                Console.WriteLine("enter the height of the box");
                str = Console.ReadLine();
                if (str == "Cancel")
                    if (str == "Cancel")
                    {
                        x = 0;
                        y = 0;
                        return false;
                    }
                if (!double.TryParse(str, out y))
                {
                    Console.WriteLine("Invalid height !");
                    continue;
                }
                return true;
            }

            


        }
    }
}
