using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheoryC.ViewModels;
using System.Threading;
using System.Windows;
using TheoryC.Common;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void VerifyAbsoluteAndConstantErrorCalculations()
        {
            // arrange
            List<double> AbsoluteErrorForEachTickList = new List<double>();
            List<bool> IsInsideTrackForEachTickList = new List<bool>();

            bool value;
            for (int i = 1; i < 11; i++)
            {
                AbsoluteErrorForEachTickList.Add(i);

                value = (i % 2 == 0);
                IsInsideTrackForEachTickList.Add(value);
            }

            // act
            double actualConstError = Statistics.ConstantError(AbsoluteErrorForEachTickList, IsInsideTrackForEachTickList);
            double actualAbsoluteError = Statistics.Mean(AbsoluteErrorForEachTickList);

            // assert
            Assert.AreEqual(-0.5, actualConstError, "Constant error is incorrect");
            Assert.AreEqual(5.5, actualAbsoluteError, "Absolute Error is incorrect");

        }

        [TestMethod]
        public void VerifyVariableErrorCalculations()
        {
            // arrange
            List<double> AbsoluteErrorForEachTickList = new List<double>();
            List<bool> IsInsideTrackForEachTickList = new List<bool>();

            bool value;
            for (int i = 1; i < 11; i++)
            {
                AbsoluteErrorForEachTickList.Add(i);

                value = (i % 2 == 0);
                IsInsideTrackForEachTickList.Add(value);
            }

            // act
            double actualVariableError = Statistics.VariableError(AbsoluteErrorForEachTickList, IsInsideTrackForEachTickList);
           
            // assert
            Assert.AreEqual(6.18466, Math.Round(actualVariableError, 5), "Variable error is incorrect");

        }

        [TestMethod]
        public void VerifyAbsoluteError()
        {
            // arrange
            List<double> AbsoluteErrorList = new List<double>();
            AbsoluteErrorList.Add(1);
            AbsoluteErrorList.Add(1);
            AbsoluteErrorList.Add(2);
            AbsoluteErrorList.Add(1);
            AbsoluteErrorList.Add(2);
            AbsoluteErrorList.Add(0);

            // act
            double actualAbsoluteError = Statistics.Mean(AbsoluteErrorList);

            // assert
            Assert.AreEqual(1.17, Math.Round(actualAbsoluteError, 2), "Absolute error is incorrect");
        }

        [TestMethod]
        public void VerifyCalculateTimeThirdsAllOnTargetOneSecond()
        {
            // arrange
            int tickCount = 33;
            double trialDuration = 1.0;
            List<bool> OnTarget = new List<bool>();
            for (int i = 0; i < tickCount; i++)
            {
                OnTarget.Add(true);
            }

            // act
            double firstTimePercentage, secondTimePercentage, thirdTimePercentage;
            Tools.GetTimeThirdPercentages(tickCount, out firstTimePercentage, out secondTimePercentage, out thirdTimePercentage);

            double time1 = Tools.CalculateTimeThirds(OnTarget, trialDuration, firstTimePercentage);
            double time2 = Tools.CalculateTimeThirds(OnTarget, trialDuration, secondTimePercentage);
            double time3 = Tools.CalculateTimeThirds(OnTarget, trialDuration, thirdTimePercentage);

            // assert
            Assert.AreEqual(0.33, Math.Round(firstTimePercentage, 2), "Absolute error is incorrect");
            Assert.AreEqual(0.33, Math.Round(secondTimePercentage, 2), "Absolute error is incorrect");
            Assert.AreEqual(0.33, Math.Round(thirdTimePercentage, 2), "Absolute error is incorrect");

            Assert.AreEqual(0.33, Math.Round(time1, 2), "Absolute error is incorrect");
            Assert.AreEqual(0.33, Math.Round(time2, 2), "Absolute error is incorrect");
            Assert.AreEqual(0.33, Math.Round(time3, 2), "Absolute error is incorrect");

        }

        [TestMethod]
        public void VerifyCalculateTimeThirdsAllOnTargetFiveSeconds()
        {
            // arrange
            int tickCount = 33;
            double trialDuration = 5.0;
            List<bool> OnTarget = new List<bool>();
            for (int i = 0; i < tickCount; i++)
            {
                OnTarget.Add(true);
            }

            // act
            double firstTimePercentage, secondTimePercentage, thirdTimePercentage;
            Tools.GetTimeThirdPercentages(tickCount, out firstTimePercentage, out secondTimePercentage, out thirdTimePercentage);

            double time1 = Tools.CalculateTimeThirds(OnTarget, trialDuration, firstTimePercentage);
            double time2 = Tools.CalculateTimeThirds(OnTarget, trialDuration, secondTimePercentage);
            double time3 = Tools.CalculateTimeThirds(OnTarget, trialDuration, thirdTimePercentage);

            double totalTime = time1 + time2 + time3; 
            double delta = Math.Abs(totalTime - trialDuration);

            // assert
            Assert.AreEqual(0.33, Math.Round(firstTimePercentage, 2), "Absolute error is incorrect");
            Assert.AreEqual(0.33, Math.Round(secondTimePercentage, 2), "Absolute error is incorrect");
            Assert.AreEqual(0.33, Math.Round(thirdTimePercentage, 2), "Absolute error is incorrect");

            Assert.AreEqual(1.667, Math.Round(time1, 3), "Absolute error is incorrect");
            Assert.AreEqual(1.667, Math.Round(time2, 3), "Absolute error is incorrect");
            Assert.AreEqual(1.667, Math.Round(time3, 3), "Absolute error is incorrect");

            Assert.IsTrue(delta <= 0.002, "total time does not all up to trial duration");
        }

        [TestMethod]
        public void VerifyCalculateTimeThirdsHalfOnTargetOneSeconds()
        {
            // arrange
            int tickCount = 33;
            double trialDuration = 1.0;
            List<bool> OnTarget = new List<bool>();
            for (int i = 0; i < tickCount; i++)
            {
                OnTarget.Add(i%2==0);
            }

            // act
            double firstTimePercentage, secondTimePercentage, thirdTimePercentage;
            Tools.GetTimeThirdPercentages(tickCount, out firstTimePercentage, out secondTimePercentage, out thirdTimePercentage);

            double time1 = Tools.CalculateTimeThirds(OnTarget, trialDuration, firstTimePercentage);
            double time2 = Tools.CalculateTimeThirds(OnTarget, trialDuration, secondTimePercentage);
            double time3 = Tools.CalculateTimeThirds(OnTarget, trialDuration, thirdTimePercentage);

            // assert
            Assert.AreEqual(0.33, Math.Round(firstTimePercentage, 2), "Absolute error is incorrect");
            Assert.AreEqual(0.33, Math.Round(secondTimePercentage, 2), "Absolute error is incorrect");
            Assert.AreEqual(0.33, Math.Round(thirdTimePercentage, 2), "Absolute error is incorrect");

            Assert.AreEqual(0.172, Math.Round(time1, 3), "Absolute error is incorrect");
            Assert.AreEqual(0.172, Math.Round(time2, 3), "Absolute error is incorrect");
            Assert.AreEqual(0.172, Math.Round(time3, 3), "Absolute error is incorrect");

            Assert.AreEqual(0.516, Math.Round(time1 + time2 + time3, 3), "total time does not all up to trial duration");
        }

        [TestMethod]
        public void VerifyCalculateTimeThirdsAllOnTargetFiveSecondsUnevenTicks()
        {
            // arrange
            int tickCount = 32;
            double trialDuration = 5.0;
            List<bool> OnTarget = new List<bool>();
            for (int i = 0; i < tickCount; i++)
            {
                OnTarget.Add(true);
            }

            // act
            double firstTimePercentage, secondTimePercentage, thirdTimePercentage;
            Tools.GetTimeThirdPercentages(tickCount, out firstTimePercentage, out secondTimePercentage, out thirdTimePercentage);

            double time1 = Tools.CalculateTimeThirds(OnTarget, trialDuration, firstTimePercentage);
            double time2 = Tools.CalculateTimeThirds(OnTarget, trialDuration, secondTimePercentage);
            double time3 = Tools.CalculateTimeThirds(OnTarget, trialDuration, thirdTimePercentage);

            // assert
            Assert.AreEqual(0.3125, firstTimePercentage, "Absolute error is incorrect");
            Assert.AreEqual(0.34375, secondTimePercentage, "Absolute error is incorrect");
            Assert.AreEqual(0.34375, thirdTimePercentage, "Absolute error is incorrect");

            Assert.AreEqual(1.562, Math.Round(time1, 3), "Absolute error is incorrect");
            Assert.AreEqual(1.719, Math.Round(time2, 3), "Absolute error is incorrect");
            Assert.AreEqual(1.719, Math.Round(time3, 3), "Absolute error is incorrect");

            Assert.AreEqual(trialDuration, Math.Round(time1 + time2 + time3, 3), "total time does not all up to trial duration");
        }

        [TestMethod]
        public void VerifyThirds()
        {
            //arrange
            double AbsoluteError1, AbsoluteError2, AbsoluteError3;
            double ConstantError1, ConstantError2, ConstantError3;
            double VariableError1, VariableError2, VariableError3;

            int TickCount = 30;
            List<double> AbsoluteErrorForEachTickList = new List<double>();
            List<bool> IsInsideTrackForEachTickList = new List<bool>();

            bool value;
            for (int j = 0; j < 3; j++)
            {
                for (int i = 1; i < 11; i++)
                {
                    AbsoluteErrorForEachTickList.Add(i);

                    value = (i % 2 == 0);
                    IsInsideTrackForEachTickList.Add(value);
                }                
            }
            
            //act

            // figure out the indexes for the 1/3
            int indexMarker = TickCount / 3;

            List<double> ABE1, ABE2, ABE3;
            Tools.BreakListIntoThirds(AbsoluteErrorForEachTickList, out ABE1, out ABE2, out ABE3);

            List<bool> IsInside1, IsInside2, IsInside3;
            Tools.BreakListIntoThirds(IsInsideTrackForEachTickList, out IsInside1, out IsInside2, out IsInside3);

            // first 1/3rd            
            AbsoluteError1 = Statistics.Mean(ABE1);
            ConstantError1 = Statistics.ConstantError(ABE1, IsInside1);
            VariableError1 = Statistics.VariableError(ABE1, IsInside1);
            
            // second 1/3rd            
            AbsoluteError2 = Statistics.Mean(ABE2);
            ConstantError2 = Statistics.ConstantError(ABE2, IsInside2);
            VariableError2 = Statistics.VariableError(ABE2, IsInside2);

            // third 1/3rd            
            AbsoluteError3 = Statistics.Mean(ABE3);
            ConstantError3 = Statistics.ConstantError(ABE3, IsInside3);
            VariableError3 = Statistics.VariableError(ABE3, IsInside3);

            //assert
            Assert.AreEqual(5.5, Math.Round(AbsoluteError1, 2), "Absolute error part 1 is incorrect");
            Assert.AreEqual(5.5, Math.Round(AbsoluteError2, 2), "Absolute error part 2 is incorrect");
            Assert.AreEqual(5.5, Math.Round(AbsoluteError3, 2), "Absolute error part 3 is incorrect");

            Assert.AreEqual(-0.5, ConstantError1, "Constant error is incorrect");
            Assert.AreEqual(-0.5, ConstantError2, "Constant error is incorrect");
            Assert.AreEqual(-0.5, ConstantError3, "Constant error is incorrect");

            Assert.AreEqual(6.18466, Math.Round(VariableError1, 5), "Variable error is incorrect");
            Assert.AreEqual(6.18466, Math.Round(VariableError2, 5), "Variable error is incorrect");
            Assert.AreEqual(6.18466, Math.Round(VariableError3, 5), "Variable error is incorrect");

        }

        [TestMethod]
        public void VerifyPopulationStandardDeviation()
        {
            // arrange
            List<double> numbers = new List<double>();

            for (int i = 1; i < 11; i++)
            {
                numbers.Add(i);
            }

            // act
            double actualResult = Statistics.PopulationStandardDeviation(numbers);

            // assert
            Assert.AreEqual(2.87, Math.Round(actualResult, 2), "Population Standard Deviation is incorrect");
        }

        [TestMethod]
        public void VerifyAllValuesAreReset()
        {
            // arrange
            MainViewModel vm = new MainViewModel();
            vm.Startup();
            vm.Trials[1].Results.AbsoluteError = 33;
            vm.Trials[1].Results.TimeOnTarget = 33;
            
            // act
            vm.StartResetExperiment();

            // assert
            double actualAE = vm.Trials[1].Results.AbsoluteError;
            double actualToT = vm.Trials[1].Results.TimeOnTarget;

            Assert.AreEqual(0, actualAE, "Absolute Error was not cleared upon starting next experiment");
            Assert.AreEqual(0, actualToT, "Time On Target was not cleared upon starting next experiment");                   
        }        

        [TestMethod]
        public void TestIsInsideCircle()
        {
            // arrange
            bool expected = true;
            Point circleCenter = new Point(100, 100);
            double circleRadius = 50;
            Point pt = new Point(115, 115);

            // act
            Tools.IsInsideCircle(pt, circleCenter, circleRadius);

            // assert
            bool actual = true;
            Assert.AreEqual(expected, actual, "Point was found outside the circle when it was inside");
        }

        [TestMethod]
        public void TestIsNotInsideCircle()
        {
            // arrange
            bool expected = false;
            Point circleCenter = new Point(100, 100);
            double circleRadius = 50;
            Point pt = new Point(200, 200);

            // act
            Tools.IsInsideCircle(pt, circleCenter, circleRadius);

            // assert
            bool actual = false;
            Assert.AreEqual(expected, actual, "Point was found inside the circle when it was outside");
        }

        [TestMethod]
        public void TestPlacementofShapeOnTrack()
        {
            //arrange
            Point trackCenter = new Point(400, 400);
            double trackRadius = 100;
            double targetRadius = 50; 

            //act
            Point actual = Tools.GetPointForPlacingTargetInStartingPosition(trackCenter, trackRadius, targetRadius);

            //assert
            Point expected = new Point(450, 350);
            Assert.AreEqual(actual, expected, "Target location is not on the track start!");
        }
    }
}
