using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Xenvious
{
    public class Global
    {
        long index;

        public Global(long index)
        {
            this.index = index;
        }

        private long GetAddressFromIndex()
        {
            return MainWindow.m.memory((Xenvious.GTA.Offsets.Editor.GlobalPTRversion + (8 * (index >> 0x12 & 0x3F))), (8 * (index & 0x3FFFF))).GetAddress();
        }

        public Global Add(long index)
        {
            //this.index += index;
            //return this;
            return new Global(this.index + index);
        }

        public Global Subtract(long index)
        {
            //this.index += index;
            //return this;
            return new Global(this.index - index);
        }

        public long GetAddress()
        {
            return GetAddressFromIndex();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T Get<T>() where T : struct
        {
            return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).Get<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool Write<T>(T value) where T : struct
        {
            return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).Write<T>(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Vector3 GetVector3()
        {
            return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).GetVector3();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool WriteVector3(Vector3 value)
        {
            return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).WriteVector3(value);
        }

        public string GetString(int size = 255, bool unicode = true)
        {
            return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).GetString(size, unicode);
        }

        public unsafe byte[] GetBytes(int length)
        {
            return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).GetBytes(length);
        }


        //    fixed (byte* b = buffer)
        //        Unsafe.Write<byte[]>(b, buffer);

        //    NtWriteVirtualMemory(procHandle, (IntPtr)address, buffer, buffer.Length, 0);
        //}

        public bool SetShort(short value)
        {
            if (MainWindow.m.IsProcOpen)
            {
                return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).SetShort(value);
            }
            return false;
        }
        public bool SetShort(string value)
        {
            if (MainWindow.m.IsProcOpen)
            {
                return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).SetShort(value);
            }
            return false;
        }


        // Write Int
        public bool SetInt(int value)
        {
            if (MainWindow.m.IsProcOpen)
            {
                return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).SetInt(value);
            }
            return false;
        }

        public bool SetInt(uint value)
        {
            if (MainWindow.m.IsProcOpen)
            {
                return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).SetInt(value);
            }
            return false;
        }

        public bool SetInt(string value)
        {
            if (MainWindow.m.IsProcOpen)
            {
                return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).SetInt(value);
            }
            return false;
        }



        // Write Long
        public bool SetLong(long value)
        {
            if (MainWindow.m.IsProcOpen)
            {
                return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).SetLong(value);
            }
            return false;
        }

        public bool SetLong(ulong value)
        {
            if (MainWindow.m.IsProcOpen)
            {
                return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).SetLong(value);
            }
            return false;
        }

        public bool SetLong(string value)
        {
            if (MainWindow.m.IsProcOpen)
            {
                return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).SetLong(value);
            }
            return false;
        }

        /// <summary>
        /// Write a String into opened Process Memory. (Only ment to be used with Numeric Datatypes)
        /// </summary>
        /// <typeparam name="T">Numeric Input Types and Strings(int, float, double, long, byte)</typeparam>
        /// <param name="value">Generic value that gets written into the opened Memory.</param>
        /// <returns></returns>
        public bool SetString<T>(T value)
        {
            if (MainWindow.m.IsProcOpen)
            {
                return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).SetString<T>(value);
            }
            return false;
        }

        public void SetString(string value, bool unicode = true)
        {
            if (MainWindow.m.IsProcOpen)
            {
                MainWindow.m.memory(GetAddressFromIndex().ToString("X")).WriteString(value, unicode);
            }
        }



        // Write Float
        public bool SetFloat(float value)
        {
            if (MainWindow.m.IsProcOpen)
            {
                return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).SetFloat(value);
            }
            return false;
        }

        public bool SetFloat(string value)
        {
            if (MainWindow.m.IsProcOpen)
            {
                return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).SetFloat(value);
            }
            return false;
        }



        // Write Double
        public bool SetDouble(double value)
        {
            if (MainWindow.m.IsProcOpen)
            {
                return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).SetDouble(value);
            }
            return false;
        }

        public bool SetDouble(string value)
        {
            if (MainWindow.m.IsProcOpen)
            {
                return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).SetDouble(value);
            }
            return false;
        }



        // Write Bytes
        public bool SetBytes(byte[] value)
        {
            if (MainWindow.m.IsProcOpen)
            {
                return MainWindow.m.memory(GetAddressFromIndex().ToString("X")).SetBytes(value);
            }
            return false;
        }

    }
}
