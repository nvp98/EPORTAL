using System;
using System.Collections.Generic;
using System.Linq;

namespace EPORTAL.Common
{
    /// <summary>
    /// Tu dong tinh per-scene heading calibration (sense + offset) cho cone tren minimap,
    /// dua tren hotspot bearings: moi navigation hotspot trong scene A co yaw trong pano,
    /// va target tro toi scene B co GPS. Tu (yaw, bearing_geo(A,B)) pairs, giai he tuyen
    /// tinh B_real = sense*yaw + c (mod 360) de tim sense (+/-1) va offset.
    ///
    /// Trinh tu uu tien moi scene:
    ///   1. Co >= 2 hotspot pairs => giai ca sense + offset chinh xac (majority vote sense)
    ///   2. Co 1 hotspot => assume sense=+1, tinh offset
    ///   3. Khong co hotspot => khong inject, dung default global (user override Alt+wheel)
    ///
    /// Output offset matches CSS cone convention: rotate(0deg) tro south
    /// => cone real_bearing = (180 + rotate_value) mod 360
    /// => rotate_target = real_bearing - 180 = sense*heading + (c - 180)
    /// => offset = c - 180
    /// </summary>
    public static class SceneHeadingCalibrator
    {
        public class Result
        {
            public Dictionary<string, double> Offsets { get; set; }
            public Dictionary<string, bool>   Inverts { get; set; }
            public Dictionary<string, SceneDiag> Diagnostics { get; set; }
            public Result()
            {
                Offsets = new Dictionary<string, double>();
                Inverts = new Dictionary<string, bool>();
                Diagnostics = new Dictionary<string, SceneDiag>();
            }
        }

        public class SceneDiag
        {
            public int SampleCount { get; set; }
            public int Sense       { get; set; }
            public double Offset   { get; set; }
            public List<DiagSample> Samples { get; set; }
            public SceneDiag() { Samples = new List<DiagSample>(); }
        }

        public class DiagSample
        {
            public string TargetId  { get; set; }
            public double Yaw       { get; set; }
            public double Bearing   { get; set; }
        }

        private class Sample
        {
            public double Yaw;
            public double Bearing;
            public string TargetId;
        }

        // KuulaHeadingSign: cach apply photos[0].options.heading lam offset
        //   "negate" (default): offset = -CameraHeading  (subtract pano→compass)
        //   "use":              offset = +CameraHeading
        //   "off":              khong dung CameraHeading lam default
        public static string KuulaHeadingSign = "negate";

        public static Result Compute(
            List<KuulaCollectionFetcher.SceneInfo> scenes,
            Dictionary<string, double[]> gpsMap)
        {
            var result = new Result();
            if (scenes == null) return result;
            if (gpsMap == null) gpsMap = new Dictionary<string, double[]>();

            foreach (var scene in scenes)
            {
                if (string.IsNullOrEmpty(scene.Uuid)) continue;  // need uuid de match client

                // STEP 1: try hotspot-bearing fit (precise) neu co du data
                bool hasHotspotFit = TryFitFromHotspots(scene, gpsMap, result);

                // STEP 2: fallback - neu khong fit duoc tu hotspot, dung options.heading
                if (!hasHotspotFit && scene.CameraHeading.HasValue && KuulaHeadingSign != "off")
                {
                    double h = scene.CameraHeading.Value;
                    double offset = (KuulaHeadingSign == "use") ? h : -h;
                    offset = NormSigned180(offset);
                    result.Offsets[scene.Uuid] = Math.Round(offset, 1);
                    var diag = new SceneDiag {
                        SampleCount = 0,
                        Sense = 1,
                        Offset = Math.Round(offset, 1)
                    };
                    result.Diagnostics[scene.Uuid] = diag;
                }
            }

            return result;
        }

        private static bool TryFitFromHotspots(
            KuulaCollectionFetcher.SceneInfo scene,
            Dictionary<string, double[]> gpsMap,
            Result result)
        {
            if (string.IsNullOrEmpty(scene.Uuid)) return false;
            if (!gpsMap.ContainsKey(scene.Uuid)) return false;
            if (scene.Spots == null || scene.Spots.Count == 0) return false;

            var fromGps = gpsMap[scene.Uuid];
            var samples = new List<Sample>();

            foreach (var spot in scene.Spots)
            {
                if (string.IsNullOrEmpty(spot.TargetId)) continue;
                if (!gpsMap.ContainsKey(spot.TargetId)) continue;
                if (spot.TargetId == scene.Id) continue;

                var toGps = gpsMap[spot.TargetId];
                var bearing = Bearing(fromGps, toGps);
                samples.Add(new Sample {
                    Yaw = Norm360(spot.Yaw),
                    Bearing = bearing,
                    TargetId = spot.TargetId
                });
            }

            if (samples.Count == 0) return false;

            int sense = DetermineSense(samples);
            double c = ComputeOffsetC(samples, sense);
            double off = NormSigned180(c - 180);

            result.Offsets[scene.Uuid] = Math.Round(off, 1);
            result.Inverts[scene.Uuid] = (sense == -1);

            var d = new SceneDiag {
                SampleCount = samples.Count,
                Sense = sense,
                Offset = Math.Round(off, 1)
            };
            foreach (var s in samples)
            {
                d.Samples.Add(new DiagSample {
                    TargetId = s.TargetId,
                    Yaw = Math.Round(s.Yaw, 1),
                    Bearing = Math.Round(s.Bearing, 1)
                });
            }
            result.Diagnostics[scene.Uuid] = d;
            return true;
        }

        // Real-world bearing (deg from N, clockwise) tu point A sang point B
        public static double Bearing(double[] from, double[] to)
        {
            double lat1 = from[0] * Math.PI / 180.0;
            double lat2 = to[0]   * Math.PI / 180.0;
            double dLng = (to[1] - from[1]) * Math.PI / 180.0;
            double y = Math.Sin(dLng) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2)
                     - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLng);
            double brng = Math.Atan2(y, x) * 180.0 / Math.PI;
            return Norm360(brng);
        }

        // Majority vote sense tu cac pair samples; tie => +1 (Kuula default convention).
        // Single-sample => +1 (khong du data de phan biet).
        private static int DetermineSense(List<Sample> samples)
        {
            if (samples.Count < 2) return 1;
            int vote = 0;
            for (int i = 0; i < samples.Count - 1; i++)
            {
                for (int j = i + 1; j < samples.Count; j++)
                {
                    double dy = ShortestDelta(samples[j].Yaw, samples[i].Yaw);
                    double dB = ShortestDelta(samples[j].Bearing, samples[i].Bearing);
                    // Skip pair gan trung (yaw delta ~0) hoac bearing delta ~0 - noise/unreliable
                    if (Math.Abs(dy) < 5 || Math.Abs(dB) < 5) continue;
                    vote += (dy * dB > 0) ? 1 : -1;
                }
            }
            return vote >= 0 ? 1 : -1;
        }

        // c = mean(B_i - sense*y_i) using circular mean de handle wrap
        private static double ComputeOffsetC(List<Sample> samples, int sense)
        {
            var cValues = samples.Select(s => Norm360(s.Bearing - sense * s.Yaw));
            return CircularMean(cValues);
        }

        private static double CircularMean(IEnumerable<double> degAngles)
        {
            double sumX = 0, sumY = 0;
            int n = 0;
            foreach (var a in degAngles)
            {
                double r = a * Math.PI / 180.0;
                sumX += Math.Cos(r);
                sumY += Math.Sin(r);
                n++;
            }
            if (n == 0) return 0;
            double mean = Math.Atan2(sumY / n, sumX / n) * 180.0 / Math.PI;
            return Norm360(mean);
        }

        public static double ShortestDelta(double target, double current)
        {
            return ((target - current) % 360 + 540) % 360 - 180;
        }

        private static double Norm360(double d) => ((d % 360) + 360) % 360;

        private static double NormSigned180(double d)
        {
            d = ((d % 360) + 360) % 360;
            return d > 180 ? d - 360 : d;
        }
    }
}
