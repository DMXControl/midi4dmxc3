using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace MidiPlugin.Utilities
{
    public static class CLRInjector
    {
        /// <summary>
        /// Replaces the method.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        public static void ReplaceMethod(MethodBase source, MethodBase dest)
        {
            if (!MethodSignaturesEqual(source, dest))
            {
                throw new ArgumentException("The method signatures are not the same.", "source");
            }
            ReplaceMethod(GetMethodAddress(source), dest);
        }

        /// <summary>
        /// Replaces the method.
        /// </summary>
        /// <param name="srcAdr">The SRC adr.</param>
        /// <param name="dest">The dest.</param>
        public static void ReplaceMethod(IntPtr srcAdr, MethodBase dest)
        {
            IntPtr destAdr = GetMethodAddress(dest);
            unsafe
            {
                if (IntPtr.Size == 8)
                {
                    ulong* d = (ulong*)destAdr.ToPointer();
                    *d = *((ulong*)srcAdr.ToPointer());
                }
                else
                {
                    uint* d = (uint*)destAdr.ToPointer();
                    *d = *((uint*)srcAdr.ToPointer());
                }
            }
        }

        /// <summary>
        /// Gets the address of the method stub
        /// </summary>
        /// <param name="methodHandle">The method handle.</param>
        /// <returns></returns>
        public static IntPtr GetMethodAddress(MethodBase method)
        {
            var fp = method.MethodHandle.GetFunctionPointer();
            if ((method is DynamicMethod))
            {
                
                var _fp = GetDynamicMethodAddress(method);
                return _fp;//GetDynamicMethodAddress(method);
                
            }

            // Prepare the method so it gets jited
            RuntimeHelpers.PrepareMethod(method.MethodHandle);

            // If 3.5 sp1 or greater than we have a different layout in memory.
            if (IsNet20Sp2OrGreater())
            {
                var _fp = GetMethodAddress20SP2(method);

                return fp;// GetMethodAddress20SP2(method);
            }


            unsafe
            {
                // Skip these
                const int skip = 10;

                // Read the method index.
                UInt64* location = (UInt64*)(method.MethodHandle.Value.ToPointer());
                int index = (int)(((*location) >> 32) & 0xFF);

                if (IntPtr.Size == 8)
                {
                    // Get the method table
                    ulong* classStart = (ulong*)method.DeclaringType.TypeHandle.Value.ToPointer();
                    ulong* address = classStart + index + skip;
                    return new IntPtr(address);
                }
                else
                {
                    // Get the method table
                    uint* classStart = (uint*)method.DeclaringType.TypeHandle.Value.ToPointer();
                    uint* address = classStart + index + skip;
                    return new IntPtr(address);
                }
            }
        }

        private static IntPtr GetDynamicMethodAddress(MethodBase method)
        {
            unsafe
            {
                RuntimeMethodHandle handle = GetDynamicMethodRuntimeHandle(method);
                byte* ptr = (byte*)handle.Value.ToPointer();
                if (IsNet20Sp2OrGreater())
                {
                    RuntimeHelpers.PrepareMethod(handle);
                    if (IntPtr.Size == 8)
                    {
                        ulong* address = (ulong*)ptr;
                        address = (ulong*)*(address + 5);
                        return new IntPtr(address + 12);
                    }
                    else
                    {
                        uint* address = (uint*)ptr;
                        address = (uint*)*(address + 5);
                        return new IntPtr(address + 12);
                    }
                }
                else
                {

                    if (IntPtr.Size == 8)
                    {
                        ulong* address = (ulong*)ptr;
                        address += 6;
                        return new IntPtr(address);
                    }
                    else
                    {
                        uint* address = (uint*)ptr;
                        address += 6;
                        return new IntPtr(address);
                    }
                }

            }
        }
        private static RuntimeMethodHandle GetDynamicMethodRuntimeHandle(MethodBase method)
        {
            if (method is DynamicMethod)
            {
                FieldInfo fieldInfo = typeof(DynamicMethod).GetField("m_method", BindingFlags.NonPublic | BindingFlags.Instance);
                RuntimeMethodHandle handle = ((RuntimeMethodHandle)fieldInfo.GetValue(method));

                return handle;
            }
            return method.MethodHandle;
        }
        private static IntPtr GetMethodAddress20SP2(MethodBase method)
        {
            unsafe
            {
                return new IntPtr(((int*)method.MethodHandle.Value.ToPointer() + 2));
            }
        }
        private static bool MethodSignaturesEqual(MethodBase x, MethodBase y)
        {
            if (x.CallingConvention != y.CallingConvention)
            {
                return false;
            }
            Type returnX = GetMethodReturnType(x), returnY = GetMethodReturnType(y);
            if (returnX != returnY)
            {
                return false;
            }
            ParameterInfo[] xParams = x.GetParameters(), yParams = y.GetParameters();
            if (xParams.Length != yParams.Length)
            {
                return false;
            }
            for (int i = 0; i < xParams.Length; i++)
            {
                if (xParams[i].ParameterType != yParams[i].ParameterType)
                {
                    return false;
                }
            }
            return true;
        }
        private static Type GetMethodReturnType(MethodBase method)
        {
            MethodInfo methodInfo = method as MethodInfo;
            if (methodInfo == null)
            {
                // Constructor info.
                throw new ArgumentException("Unsupported MethodBase : " + method.GetType().Name, "method");
            }
            return methodInfo.ReturnType;
        }
        private static bool IsNet20Sp2OrGreater()
        {
            return Environment.Version.Major == FrameworkVersions.Net20SP2.Major &&
                Environment.Version.MinorRevision >= FrameworkVersions.Net20SP2.MinorRevision;
        }
    }

    public static class FrameworkVersions
    {
        public static readonly Version Net35 = new Version(3, 5, 21022, 8);
        public static readonly Version Net35SP1 = new Version(3, 5, 30729, 1);
        public static readonly Version Net30 = new Version(3, 0, 4506, 30);
        public static readonly Version Net30SP1 = new Version(3, 0, 4506, 648);
        public static readonly Version Net30SP2 = new Version(3, 0, 4506, 2152);
        public static readonly Version Net20 = new Version(2, 0, 50727, 42);
        public static readonly Version Net20SP1 = new Version(2, 0, 50727, 1433);
        public static readonly Version Net20SP2 = new Version(2, 0, 50727, 3053);

        //1.1	Original release	1.1.4322.573
        //1.1	Service Pack 1	1.1.4322.2032
        //1.1	Service Pack 1 (Windows Server 2003 32-bit version*)	1.1.4322.2300
        //1.0	Original release	1.0.3705.0
        //1.0	Service Pack 1	1.0.3705.209
        //1.0	Service Pack 2	1.0.3705.288
        //1.0	Service Pack 3	1.0.3705.6018
    }
}
