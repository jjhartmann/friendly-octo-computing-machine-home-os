using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeOS.Hub.Common.Bolt.Apps.PreHeat
{
    class MainClass
    {
        static void Main(string[] args)
        {
            int len = 96000;
            string outputFile;

            outputFile = ".\\smart";
            SmartPreHeat spreheat = new SmartPreHeat(null, 5, outputFile);
            spreheat.PredictOccupancy(0, len);

            CreateDayMax(outputFile, 3);
            CreateDayMax(outputFile, 4);
            CreateDayMax(outputFile, 5);


            outputFile = ".\\optimal";
            OptimalPreHeat preheat = new OptimalPreHeat(null, 5, outputFile);
            preheat.PredictOccupancy(0, len);

            CreateDayMax(outputFile, 3);
            CreateDayMax(outputFile, 4);
            CreateDayMax(outputFile, 5);


            outputFile = ".\\naive";
            NaivePreHeat npreheat = new NaivePreHeat(null, 5, outputFile);
            npreheat.PredictOccupancy(0, len);

            CreateDayMax(outputFile, 3);
            CreateDayMax(outputFile, 4);
            CreateDayMax(outputFile, 5);


            
            

        }

        static void CreateDayMax(string filePath, int index)
        {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(filePath);
            int maxRet = -1;

            while ((line = file.ReadLine()) != null)
            {
                string[] words = line.Split(' ');

                int slot = Int32.Parse(words[2]);
                slot--;

                if (Int32.Parse(words[index]) > maxRet)
                    maxRet = Int32.Parse(words[index]);

                if (slot % 96 == 0)
                {
                    Console.WriteLine("{0},{1}", slot / 96, (float)(maxRet / 10000));

                    using (StreamWriter w = File.AppendText(filePath+ "-daymax-"+index ))
                    {
                        w.WriteLine("{0},{1}", slot / 96, (float)(maxRet / 10000));
                    }
                    maxRet = -1;
                }

            }

            file.Close();

        }

    }
}
