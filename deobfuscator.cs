using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.Win32;

namespace MyKey
{
    class deobfuscator
    {
        public static string getSerial(string from = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion", string valueName = "DigitalProductId")
        {
            RegistryKey hive = null;
            RegistryKey key = null;
            try
            {
                string result = "";
                hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
                key = hive.OpenSubKey(from, false);
                RegistryValueKind k = RegistryValueKind.Unknown;
                try { k = key.GetValueKind(valueName); }
                catch (Exception) { }

                if (k == RegistryValueKind.Unknown)
                {
                    key.Close();
                    hive.Close();
                    hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    key = hive.OpenSubKey(from, false);
                    try { k = key.GetValueKind(valueName); }
                    catch (Exception) { }
                }

                if (k == RegistryValueKind.Binary)
                {
                    int pivot = 0;
                    byte[] bytes = (byte[])key.GetValue(valueName);
                    int[] ints = new int[16];
                    for (int i = 52; i < 67; ++i) ints[i - 52] = bytes[i];
                    for (int i = 0; i < 25; ++i)
                    {
                        pivot = 0;
                        for (int j = 14; j >= 0; --j)
                        {
                            pivot <<= 8;
                            pivot ^= ints[j];
                            ints[j] = ((int)Math.Truncate(pivot / 24.0));
                            pivot %= 24;
                        }
                        result = possible_chars[pivot] + result;
                        if ((i % 5 == 4) && (i != 24)) result = "-" + result;
                    }
                }
                return result;
            }
            catch (Exception e) { return e.Message; }
            finally
            {
                if (null != key) key.Close();
                if (null != hive) hive.Close();
            }
        }

        private static readonly string possible_chars = "BCDFGHJKMPQRTVWXY2346789";
    }
}
