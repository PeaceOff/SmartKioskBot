﻿using MongoDB.Bson;
using MongoDB.Driver;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace SmartKioskBot.Logic
{
    public static class Comparator
    {
        public static Product GetBestProduct (List<Product> products)
        {
            int bestCPUIndex = GetBestCPU(products);

            return products[bestCPUIndex];
           
        }

        public static int GetBestCPU (List<Product> products)
        {
            List<Comparable.CPU> cpus = new List<Comparable.CPU>();

            Regex dualRegex = new Regex(@"dual", RegexOptions.IgnoreCase);
            Regex quadRegex = new Regex(@"quad", RegexOptions.IgnoreCase);

            for (int i = 0; i < products.Count; i++)
            {
                Product currentProduct = products[i];
                int numOfCores = 0;

                if(currentProduct.CoreNr != null)
                {
                    if (dualRegex.Match(currentProduct.CoreNr).Length != 0) // match dual core (2 cores)
                    {
                        numOfCores = 2;
                    }

                    if (quadRegex.Match(currentProduct.CoreNr).Length != 0) // match quad core (4 cores)
                    {
                        numOfCores = 4;
                    }
                }

                float cpuSpeed = 0;

                if (currentProduct.CPUSpeed != null)
                {
                   cpuSpeed = float.Parse(currentProduct.CPUSpeed, System.Globalization.CultureInfo.InvariantCulture);
                }

                cpus.Add(new Comparable.CPU(numOfCores, cpuSpeed));
            }

            return GetBestPart(cpus);
        }
        public static int GetBestRAM(List<Product> products)
        {
            List<Comparable.RAM> rams = new List<Comparable.RAM>();

            Regex numberRegex = new Regex(@"\d+"); 

            for (int i = 0; i < products.Count; i++)
            {
                Product currentProduct = products[i];
                int memory;

                memory = int.Parse(numberRegex.Match(currentProduct.RAM).Value);

                rams.Add(new Comparable.RAM(memory));
            }

            return GetBestPart(rams);
        }

        public static int GetBestScreen(List<Product> products)
        {
            List<Comparable.Screen> screens = new List<Comparable.Screen>();

            Regex numberRegex = new Regex(@"\d+");
            Regex negRegex = new Regex(@"não", RegexOptions.IgnoreCase);
            Regex posRegex = new Regex(@"sim", RegexOptions.IgnoreCase);

            for (int i = 0; i < products.Count; i++)
            {
                Product currentProduct = products[i];
                int resolution_x, resolution_y;
                float diagonal;
                bool touch = false;
                
                diagonal = float.Parse(numberRegex.Match(currentProduct.ScreenDiagonal).Value);
                Match resMatch = numberRegex.Match(currentProduct.ScreenResolution);
                resolution_x = int.Parse(resMatch.Value);
                resolution_y = int.Parse(resMatch.NextMatch().Value);

                if (negRegex.Match(currentProduct.TouchScreen).Length != 0)
                {
                    touch = false;
                } else if (posRegex.Match(currentProduct.TouchScreen).Length != 0)
                    {
                        touch = true;
                    }

                screens.Add(new Comparable.Screen(diagonal, resolution_x, resolution_y, touch));
            }

            return GetBestPart(screens);
        }

        public static int GetBestPart<T> (List<T> comparables) where T : Comparable
        {
            int indexOfBestComparable = -1;

            for(int i = 0; i < comparables.Count - 1; i++)
            {
                int comparisonResult = comparables[i].CompareTo(comparables[i + 1]);

                if (comparisonResult > 0)
                {
                    indexOfBestComparable = i;
                }
                else if (comparisonResult < 0)
                {
                    indexOfBestComparable = i + 1;
                }
            }

            return indexOfBestComparable;
        }

        public static void Test()
        {

            var collection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.CollectionName);
            var builder = Builders<Product>.Filter;

            // var filter1 = builder.Eq("_id", ObjectId.Parse("5ad6628086e5482fb04ea97b"));
            // var filter2 = builder.Eq("_id", ObjectId.Parse("5ad6628086e5482fb04ea97b"));

            // var product1 = collection.Find(filter1).FirstOrDefault();
            // var product2 = collection.Find(filter2).FirstOrDefault();

            Product product1 = new Product();
            product1.CoreNr = "Dual Core";
            product1.CPUSpeed = "2.7";

            Product product2 = new Product();
            product2.CoreNr = "Quad Core";
            product2.CPUSpeed = "2.7";

            Product winner = GetBestProduct(new List<Product>() { product1, product2 });
            Debug.Write(winner.CoreNr);
        }
    }
}