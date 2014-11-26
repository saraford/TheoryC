using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Threading;

namespace TheoryC.Devices
{
    public class KinectDevice
    {
        KinectSensor kinectSensor;
        CoordinateMapper cm;
        MultiSourceFrameReader reader;
        IList<Body> bodies;
        private const float InferredZPositionClamp = 0.1f;
        public Point rightHandTip;
//        public double rightHandTipDepth;
        public Point leftHandTip;
        public double leftHandTipDepth;
        public PointF leanAmount;
        private List<Tuple<JointType, JointType>> bones;
        private const double JointThickness = 5;
        public bool useRightShoulder = true;
        public Point leftShoulderCenter; // by default use the center of the screen
        public Point rightShoulderCenter; // by default use the center of the screen
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Brush inferredBoneBrush = Brushes.Gray;
        private readonly Brush windowShowingBrush = Brushes.LightBlue; //(SolidColorBrush)(new BrushConverter().ConvertFrom("#FF3843C4")); // (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD0D8E0"));
        private readonly Brush windowDefaultBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));
        Canvas bodyCanvas;
        Image kinectVideoImage;
        ViewModels.MainViewModel ViewModel;

        public KinectDevice()
        {
        }

        #region "Kinect Setup and Teardown"

        internal bool IsKinectAvailable()
        {
            kinectSensor = KinectSensor.GetDefault();

            kinectSensor.Open();

            // need to give it time to find the sensor
            Thread.Sleep(2000);

            return this.kinectSensor.IsAvailable;
        }

        internal void InitializeKinect(Canvas bodyCanvas, Image kinectVideoImage, ViewModels.MainViewModel ViewModel)
        {
            this.bodyCanvas = bodyCanvas;
            this.kinectVideoImage = kinectVideoImage;
            this.ViewModel = ViewModel;

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // get the coordinate mapper
            this.cm = this.kinectSensor.CoordinateMapper;

            // in case we want to show the skeleton later
            CreateBonesList();

            reader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);
            reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

            this.ViewModel.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                : Properties.Resources.NoSensorStatusText;
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.ViewModel.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    bodyCanvas.Children.Clear();

                    bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(bodies);

                    this.TrackSkeleton(bodies);
                }
            }

            // Color - get this last for performance issues
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    kinectVideoImage.Source = frame.ToBitmap();
                }
            }

        }

        private void TrackSkeleton(IList<Body> bodies)
        {
            foreach (Body body in bodies)
            {
                // find the first tracked body - if there are more than one, this will fail miserably
                if (body.IsTracked)
                {
                    IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                    // convert the joint points to depth (display) space
                    Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                    foreach (JointType jointType in joints.Keys)
                    {
                        // sometimes the depth(Z) of an inferred joint may show as negative
                        // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                        // this is only needed for showing the skeleton
                        CameraSpacePoint position = joints[jointType].Position;
                        if (position.Z < 0)
                        {
                            position.Z = InferredZPositionClamp;
                        }

                        ColorSpacePoint colorSpacePoint = this.cm.MapCameraPointToColorSpace(position);
                        jointPoints[jointType] = new Point(colorSpacePoint.X, colorSpacePoint.Y);

                        // track lean
                        if (body.LeanTrackingState == TrackingState.Tracked)
                        {
                            // Leaning left and right corresponds to X movement; leaning forward and back corresponds to Y movement. 
                            // The values range between -1 and 1 in both drections, where 1 roughly corresponds to 45 degrees of lean.
                            leanAmount = body.Lean;
                        }

                        // track specific joints
                        if (jointType == JointType.HandTipRight)
                        {
                            rightHandTip.X = ConvertToCanvasX(jointPoints[jointType].X);
                            rightHandTip.Y = ConvertToCanvasY(jointPoints[jointType].Y);
                            
                            // The depth of an object 1 unit = 1 meter
                            // http://msdn.microsoft.com/en-us/library/windowspreview.kinect.cameraspacepoint.aspx
                            this.ViewModel.HandDepth = position.Z;
                            
                            // draws a circle for the tip of the hand
                            if (this.ViewModel.ShowFingerTip)
                            {
                                Brush drawBrush = null;

                                TrackingState trackingState = joints[jointType].TrackingState;

                                if (trackingState == TrackingState.Tracked)
                                {
                                    drawBrush = this.trackedJointBrush;
                                }
                                else if (trackingState == TrackingState.Inferred)
                                {
                                    drawBrush = this.inferredJointBrush;
                                }

                                if (drawBrush != null)
                                {
                                    this.DrawEllipse(drawBrush, jointPoints[jointType], JointThickness);
                                }
                            }

                        }

                        else if (jointType == JointType.HandTipLeft)
                        {
                            leftHandTip.X = ConvertToCanvasX(jointPoints[jointType].X);
                            leftHandTip.Y = ConvertToCanvasY(jointPoints[jointType].Y);
                            leftHandTipDepth = position.Z;
                        }

                        else if (jointType == JointType.ElbowRight)
                        {
                            rightShoulderCenter.X = ConvertToCanvasX(jointPoints[jointType].X);
                            rightShoulderCenter.Y = ConvertToCanvasY(jointPoints[jointType].Y);
                        }

                        else if (jointType == JointType.ShoulderLeft)
                        {
                            leftShoulderCenter.X = ConvertToCanvasX(jointPoints[jointType].X);
                            leftShoulderCenter.Y = ConvertToCanvasY(jointPoints[jointType].Y);
                        }

                    }

                    // here we go
                    this.ViewModel.InputPosition = rightHandTip;

                    // are we ready? either hand will work
                    if (rightHandTip.X > 0)
                    {
                        this.ViewModel.IsKinectTracking = true;
                        // do I need the else to set to false?
                    }

                    // if we want to show the skeleton
                    if (this.ViewModel.ShowSkeleton)
                    {
                        this.DrawBody(joints, jointPoints);
                    }
                }
            }

        }

        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints)
        {

            // Draw the bones
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2);
            }

            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {

                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    this.DrawEllipse(drawBrush, jointPoints[jointType], JointThickness);
                }

            }
        }

        private Point ConvertToCanvas(Point point)
        {

            // color stream is coming in at 1920(w)×1080(h) 
            Point pt = new Point();
            pt.X = (double)(point.X / 1920.0 * bodyCanvas.ActualWidth);
            pt.Y = (double)(point.Y / 1080.0 * bodyCanvas.ActualHeight);

            //BUGBUG
            if (pt.X < 0 || pt.Y < 0)
            {
                pt.X = InferredZPositionClamp;
                pt.Y = InferredZPositionClamp;
            }


            return pt;
        }

        private double ConvertToCanvasX(double x)
        {
            // color stream is coming in at 1920(w)×1080(h) 
            return (double)(x / 1920.0 * bodyCanvas.ActualWidth);
        }

        private double ConvertToCanvasY(double y)
        {
            return (double)(y / 1080.0 * bodyCanvas.ActualHeight);
        }


        private void DrawEllipse(Brush drawBrush, Point point, double JointThickness)
        {
            Ellipse jointEllipse = new Ellipse();
            jointEllipse.Stroke = drawBrush;
            jointEllipse.StrokeThickness = 5;
            jointEllipse.Fill = drawBrush;
            jointEllipse.Height = JointThickness;
            jointEllipse.Width = JointThickness;

            Point pt = ConvertToCanvas(point);

            Canvas.SetLeft(jointEllipse, pt.X - (JointThickness / 2));
            Canvas.SetTop(jointEllipse, pt.Y - (JointThickness / 2));

            bodyCanvas.Children.Add(jointEllipse);
        }

        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Brush drawBrush = this.inferredBoneBrush;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawBrush = trackedJointBrush; // same tracked color
            }

            this.DrawLine(drawBrush, jointPoints[jointType0], jointPoints[jointType1]);
        }

        private void DrawLine(Brush drawBrush, Point point1, Point point2)
        {
            Line boneLine = new Line();
            boneLine.Stroke = drawBrush;
            boneLine.StrokeThickness = 2;

            Point pt1 = ConvertToCanvas(point1);
            Point pt2 = ConvertToCanvas(point2);

            boneLine.X1 = pt1.X;
            boneLine.Y1 = pt1.Y;

            boneLine.X2 = pt2.X;
            boneLine.Y2 = pt2.Y;

            bodyCanvas.Children.Add(boneLine);
        }

        #endregion //Kinect Setup and Teardown


        private void CreateBonesList()
        {
            // a bone defined as a line between two joints
            this.bones = new List<Tuple<JointType, JointType>>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

        }


        public void Shutdown()
        {
            // shut downs kinect
            if (reader != null)
            {
                reader.Dispose();
            }

            if (kinectSensor != null)
            {
                kinectSensor.Close();
            }

        }
    }
}
