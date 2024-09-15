using LibNoise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;

namespace BurstLibNoise.Operator
{
    class Terrace : LibNoise.Operator.Terrace, BurstModuleBase
    {
        public ModuleData GetData(int[] sources)
        {
            float[] points = ControlPoints.Select(x => (float)x).ToArray();
            return new ModuleData(ModuleType.Terrace, sources, IsInverted ? 1f : 0f,(float)points.Length , points.Length == 1 ? points[0]:0, points.Length == 2 ? points[1] : 0, points.Length == 3 ? points[2] : 0, points.Length == 4 ? points[3] : 0);
        }

        // Must be included in each file because Unity does not support C# 8.0 not supported yet (default interface implementation)
        public BurstModuleBase Source(int i)
        {
            return (BurstModuleBase)Modules[i];
        }

        public Terrace() : base() { }

        public Terrace(BurstModuleBase module) : base((ModuleBase)module) { }


        private static float getValue(ModuleData moduleData, int index)
        {
            if(index == 0)
            {
                return moduleData.param3;
            }
            if (index == 1)
            {
                return moduleData.param4;
            }
            if (index == 2)
            {
                return moduleData.param5;
            }
            if (index == 3)
            {
                return moduleData.param6;
            }
            return 0;
        }

        public static float GetBurstValue(float x, float y, float z, NativeArray<ModuleData> data, int dataIndex)
        {
            ModuleData moduleData = data[dataIndex];
            bool isInverted = moduleData.param1 == 1f;
            int controlSize = (int)moduleData.param2;

            var smv = BurstModuleManager.GetBurstValue(x, y, z, data, moduleData.Source(0));

            int ip;
            for (ip = 0; ip < controlSize; ip++)
            {
                if (smv < getValue(moduleData, ip))
                {
                    break;
                }
            }
            var i0 = math.clamp(ip - 1, 0, controlSize);
            var i1 = math.clamp(ip, 0, controlSize);

            if (i0 == i1)
            {
                return getValue(moduleData, i1);
            }

            var v0 = getValue(moduleData, i0);
            var v1 = getValue(moduleData, i1);
            var a = (smv - v0) / (v1 - v0);

            if (isInverted)
            {
                a = 1.0f - a;
                var t = v0;
                v0 = v1;
                v1 = t;
            }
            a *= a;
            return (float)Utils.InterpolateLinear((double)v0, (double)v1, (double)a);
        }

    }
}
