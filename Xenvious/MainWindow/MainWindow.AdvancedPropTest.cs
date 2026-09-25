using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Old advanced prop placement test buttons.
    public partial class MainWindow
    {
        private void ddadvancedproptype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private static XenVector3 SizeFromMinMax(XenVector3 min, XenVector3 max)
        {
            return new XenVector3(max.X - min.X, max.Y - min.Y, max.Z - min.Z);
        }

        private static float[,] RotationMatrixFromEuler(float pitchDeg, float rollDeg, float yawDeg)
        {
            float pitch = pitchDeg * (float)Math.PI / 180f;
            float roll = rollDeg * (float)Math.PI / 180f;
            float yaw = yawDeg * (float)Math.PI / 180f;

            // Rotationsmatrizen
            float[,] Rx = {
            {1, 0, 0},
            {0, (float)Math.Cos(roll), -(float)Math.Sin(roll)},
            {0, (float)Math.Sin(roll), (float)Math.Cos(roll)}
        };

            float[,] Ry = {
            {(float)Math.Cos(pitch), 0, (float)Math.Sin(pitch)},
            {0, 1, 0},
            {-(float)Math.Sin(pitch), 0, (float)Math.Cos(pitch)}
        };

            float[,] Rz = {
            {(float)Math.Cos(yaw), -(float)Math.Sin(yaw), 0},
            {(float)Math.Sin(yaw), (float)Math.Cos(yaw), 0},
            {0, 0, 1}
        };

            // R = Rz * Ry * Rx
            float[,] Rzy = MatMul(Rz, Ry);
            float[,] R = MatMul(Rzy, Rx);

            return R;
        }

        private static float[,] MatMul(float[,] A, float[,] B)
        {
            float[,] result = new float[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++)
                        result[i, j] += A[i, k] * B[k, j];
            return result;
        }

        private static XenVector3 RotateVector(XenVector3 v, float[,] R)
        {
            return new XenVector3(
                R[0, 0] * v.X + R[0, 1] * v.Y + R[0, 2] * v.Z,
                R[1, 0] * v.X + R[1, 1] * v.Y + R[1, 2] * v.Z,
                R[2, 0] * v.X + R[2, 1] * v.Y + R[2, 2] * v.Z
            );
        }

        /// <summary>
        /// Berechnet die Weltposition von Modell 2 basierend auf Modell 1.
        /// </summary>
        public static XenVector3 GetAdjacentPositionWithRotation(
            XenVector3 p1,
            XenVector3 min1, XenVector3 max1,
            XenVector3? min2 = null, XenVector3? max2 = null,
            float pitch = 0f, float roll = 0f, float yaw = 0f,
            string axis = "right",
            float gap = 0f
        )
        {
            // Größe der Bounding Boxen
            XenVector3 size1 = SizeFromMinMax(min1, max1);
            XenVector3 size2 = (min2.HasValue && max2.HasValue)
                ? SizeFromMinMax(min2.Value, max2.Value)
                : size1;

            // Lokale Achse bestimmen
            XenVector3 axisLocal;
            float size1Axis, size2Axis;
            if (axis == "forward")
            {
                axisLocal = new XenVector3(0, 1, 0);
                size1Axis = size1.Y;
                size2Axis = size2.Y;
            }
            else if (axis == "up")
            {
                axisLocal = new XenVector3(0, 0, 1);
                size1Axis = size1.Z;
                size2Axis = size2.Z;
            }
            else
            {
                axisLocal = new XenVector3(1, 0, 0);
                size1Axis = size1.X;
                size2Axis = size2.X;
            }

            // Offset-Distanz berechnen
            float offsetDist = (size1Axis / 2f) + (size2Axis / 2f) + gap;

            // Rotationsmatrix erzeugen
            float[,] R = RotationMatrixFromEuler(pitch, roll, yaw);

            // Lokale Achse in Weltkoordinaten rotieren
            XenVector3 axisWorld = RotateVector(axisLocal, R);

            // Offset in Weltkoordinaten
            XenVector3 offset = axisWorld * offsetDist;

            // Finale Position von Modell 2
            return p1 + offset;
        }

        private void BtnApropGD_Click(object sender, RoutedEventArgs e)
        {
            int lastpropnum = new Global(GTA.Offsets.Editor.Props.number).Get<int>() - 1;
            int model = new Global(GTA.Offsets.Editor.Props.model + lastpropnum * GTA.Offsets.Editor.Props.NEXT).Get<int>();
            new Global(GTA.Offsets.Editor.custom_dimension_model).SetInt(model);

            creatorRefresh();
        }

        private void BtnApropTest_Click(object sender, RoutedEventArgs e)
        {
            int lastpropnum = new Global(GTA.Offsets.Editor.Props.number).Get<int>() - 1;

            XenVector3 vector3loc1 = new XenVector3(
                new Global(GTA.Offsets.Editor.Props.loc + 0 + lastpropnum * GTA.Offsets.Editor.Props.NEXT).Get<float>(),
                new Global(GTA.Offsets.Editor.Props.loc + 1 + lastpropnum * GTA.Offsets.Editor.Props.NEXT).Get<float>(),
                new Global(GTA.Offsets.Editor.Props.loc + 2 + lastpropnum * GTA.Offsets.Editor.Props.NEXT).Get<float>());

            XenVector3 vector3rot1 = new XenVector3(
                new Global(GTA.Offsets.Editor.Props.vrot + 0 + lastpropnum * GTA.Offsets.Editor.Props.NEXT).Get<float>(),
                new Global(GTA.Offsets.Editor.Props.vrot + 1 + lastpropnum * GTA.Offsets.Editor.Props.NEXT).Get<float>(),
                new Global(GTA.Offsets.Editor.Props.vrot + 2 + lastpropnum * GTA.Offsets.Editor.Props.NEXT).Get<float>());

            int model = new Global(GTA.Offsets.Editor.Props.model + lastpropnum * GTA.Offsets.Editor.Props.NEXT).Get<int>();

            XenVector3 min = new Global(GTA.Offsets.Editor.custom_dimension_min).Get<XenVector3>();
            XenVector3 max = new Global(GTA.Offsets.Editor.custom_dimension_max).Get<XenVector3>();

            int newpropnum = lastpropnum + 1;

            // Position von Modell 2 berechnen
            XenVector3 p2 = GetAdjacentPositionWithRotation(
                vector3loc1, min, max,
                pitch: vector3rot1.X, roll: vector3rot1.Y, yaw: vector3rot1.Z,
                axis: "right",
                gap: 0f
            );

            PlaceProp(model, p2, vector3rot1);
            creatorRefresh();
        }

        public void PlaceProp(int model, XenVector3 loc, XenVector3 rot)
        {
            int newpropnum = new Global(GTA.Offsets.Editor.Props.number).Get<int>();
            new Global(GTA.Offsets.Editor.Props.number).SetInt(newpropnum + 1);
            new Global(GTA.Offsets.Editor.Props.model + newpropnum * GTA.Offsets.Editor.Props.NEXT).SetInt(model);
            new Global(GTA.Offsets.Editor.Props.loc + 0 + newpropnum * GTA.Offsets.Editor.Props.NEXT).SetFloat(loc.X);
            new Global(GTA.Offsets.Editor.Props.loc + 1 + newpropnum * GTA.Offsets.Editor.Props.NEXT).SetFloat(loc.Y);
            new Global(GTA.Offsets.Editor.Props.loc + 2 + newpropnum * GTA.Offsets.Editor.Props.NEXT).SetFloat(loc.Z);
            new Global(GTA.Offsets.Editor.Props.vrot + 0 + newpropnum * GTA.Offsets.Editor.Props.NEXT).SetFloat(rot.X);
            new Global(GTA.Offsets.Editor.Props.vrot + 1 + newpropnum * GTA.Offsets.Editor.Props.NEXT).SetFloat(rot.Y);
            new Global(GTA.Offsets.Editor.Props.vrot + 2 + newpropnum * GTA.Offsets.Editor.Props.NEXT).SetFloat(rot.Z);
        }
    }
}
