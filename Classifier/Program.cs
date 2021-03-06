﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Accord.Statistics.Models.Regression;
using Accord.Statistics.Models.Regression.Fitting;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Math.Optimization.Losses;
using System.IO;

namespace Classifier
{
    class Program
    {
        static private void writeCSVfile(RegressionData data, double[][] r2)
        {
            const string OUTPUT_FILE = "WeightMatrix.csv";
            string textForFile = "";
            for (int i = 0; i < data.InterestOrder.Count; i++)
            {
                textForFile += i == data.InterestOrder.Count - 1 ? data.InterestOrder[i].ToString() + Environment.NewLine : data.InterestOrder[i].ToString() + ", ";
            }
            for (int i = 0; i < data.MajorOrder.Count; i++)
            {
                textForFile += i == data.MajorOrder.Count - 1 ? data.MajorOrder[i].ToString() + Environment.NewLine : data.MajorOrder[i].ToString() + ", ";
            }
            for (int i = 0; i < r2.Length; i++)
            {
                for (int j = 0; j < r2[i].Length; j++)
                {
                    textForFile += j == r2[i].Length - 1 ? r2[i][j].ToString() : r2[i][j].ToString() + ", ";
                }
                textForFile += Environment.NewLine;
            }
            File.WriteAllText(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + OUTPUT_FILE, textForFile);
        }

        static private void GenerateCSFile(RegressionData data, double[][] r2)
        {
            const string OUTPUT_FILE = "\\Weights.cs";
            StringBuilder CSfileContents = new StringBuilder(10000);
            CSfileContents.Append(Environment.NewLine);
            CSfileContents.Append("//Generated by TamuyalClassifier: https://github.com/Kaikat/TamuyalClassifier");
            CSfileContents.Append(Environment.NewLine);
            CSfileContents.Append("using System;");
            CSfileContents.Append(Environment.NewLine);
            CSfileContents.Append(Environment.NewLine);
            CSfileContents.Append("namespace WebApplication1.Controllers");
            CSfileContents.Append(Environment.NewLine);
            CSfileContents.Append("{");
            CSfileContents.Append(Environment.NewLine);
            CSfileContents.Append("\tpublic static class Weights");
            CSfileContents.Append(Environment.NewLine);
            CSfileContents.Append("\t{");
            CSfileContents.Append(Environment.NewLine);
            CSfileContents.Append("\t\tpublic static readonly double[][] Matrix =");
            CSfileContents.Append(Environment.NewLine);
            CSfileContents.Append("\t\t{");
            CSfileContents.Append(Environment.NewLine);

            //append matrix values
            string textForFile = "";
            List<Interest> interestInCodeOrder = Enum.GetValues(typeof(Interest)).OfType<Interest>().ToList();
            List<Major> majorsInCodeOrder = Enum.GetValues(typeof(Major)).OfType<Major>().ToList();

            for (int i = 0; i < interestInCodeOrder.Count; i++) //topics
            {
                textForFile += "\t\t\tnew double[] { ";
                for (int j = 0; j < majorsInCodeOrder.Count; j++) //majors
                {
                    if (j % 4 == 0)
                    {
                        textForFile += Environment.NewLine + "\t\t\t\t";
                    }
                    int indexI = data.InterestOrder.IndexOf(interestInCodeOrder[i]);
                    int indexJ = data.MajorOrder.IndexOf(majorsInCodeOrder[j]);
                    textForFile += j == r2[i].Length - 1 ? r2[indexI][indexJ].ToString() : r2[indexI][indexJ].ToString() + ", ";
                }
                textForFile += i == r2.Length - 1 ? Environment.NewLine + "\t\t\t}" : Environment.NewLine + "\t\t\t},";
                textForFile += Environment.NewLine;
            }

            CSfileContents.Append(textForFile);
            CSfileContents.Append("\t\t};");
            CSfileContents.Append(Environment.NewLine);
            CSfileContents.Append("\t};");
            CSfileContents.Append(Environment.NewLine);
            CSfileContents.Append("};");
            File.WriteAllText(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + 
                OUTPUT_FILE, CSfileContents.ToString());
        }

        //EXAMPLE: http://accord-framework.net/docs/html/T_Accord_Statistics_Models_Regression_Linear_MultivariateLinearRegression.htm
        static void Main(string[] args)
        {
            CSV_Parser parser = new CSV_Parser();
            RegressionData data = parser.ParseDataFile();

            // Use Ordinary Least Squares to create the regression
            OrdinaryLeastSquares ols = new OrdinaryLeastSquares();

            // Now, compute the multivariate linear regression:
            MultivariateLinearRegression regression = ols.Learn(data.InterestRatings, data.MajorRatings);

            // We can obtain predictions using
            double[][] predictions = regression.Transform(data.InterestRatings);

            // The prediction error is
            double error = new SquareLoss(data.MajorRatings).Loss(predictions); // 0

            // We can also check the r-squared coefficients of determination:
            //double[] r2 = regression.CoefficientOfDetermination(topicRatings, majorRatings);
            double[][] r2 = regression.Weights;
            Console.WriteLine("WEIGHTS:");
            //writeCSVfile(data, r2);
            GenerateCSFile(data, r2);

            Console.WriteLine("Coefficient Of Determination");
            double[] r3 = regression.CoefficientOfDetermination(data.InterestRatings, data.MajorRatings);
            for(int i = 0; i < r3.Length; i++)
            {
                Console.WriteLine(r3[i]);
            }
            
            Console.Read();
        }
    }
}
