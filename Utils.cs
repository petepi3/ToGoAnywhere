using System;
using System.Collections.Generic;
using System.Linq;
using Petepi.TGA.Grid;
using UnityEngine;

namespace Petepi.TGA
{
    public static class Utils
    {
        private static Dictionary<Resource, string> _resourceLocalizationKeys = new ()
        {
            { Resource.Wood, "Resource_Wood" },
            { Resource.Stone, "Resource_Stone" },
            { Resource.None, "Nothing" },
        };

        public static string GetResourceLocalizationKey(Resource resource)
        {
            return _resourceLocalizationKeys[resource];
        }
        
        public static float AprSqrt(float x, float error)
        {
            float approx = x/2;

            while (Mathf.Abs(approx*approx - x) > error)
            {
                approx = (approx + x/approx)/2;
            }

            return approx;
        }

        public static float AprSqrtTest(float x, float error, float m, out int i, out bool maxedOut)
        {
            float approx = x/m;
            maxedOut = false;
            i = 0;
            while (Mathf.Abs(approx*approx - x) > error)
            {
                i++;
                if (i > 10)
                {
                    maxedOut = true;
                    return approx;
                }
                approx = (approx + x/approx)/m;
            }

            return approx;
        }

        public static float AprSqrt2(float x)
        {
            float approx = x/2f;
            approx = (approx + x/approx)/2f;
            return approx;
        }
        
        public static Resource[] AllResources => ((Resource[])Enum.GetValues(typeof(Resource))).Where(r => r != Resource.None).ToArray();
    }
}