/*
    Socks5 - A full-fledged high-performance socks5 proxy server written in C#. Plugin support included.
    Copyright (C) 2016 ThrDev

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace socks5.Plugin
{
    public class PluginLoader
    {
        public static bool LoadPluginsFromDisk { get; set; }
        //load plugin staticly.
        private static List<object> Plugins = new List<object>();
        public static void LoadPlugins()
        {
            if (loaded) return;
            try
            {
                try
                {
                    foreach (Type f in Assembly.GetExecutingAssembly().GetTypes())
                    {
                        try
                        {
                            if (!CheckType(f))
                            {
                                object type = Activator.CreateInstance(f);
                                Plugins.Push(type);
#if DEBUG
                                Console.WriteLine("Loaded Embedded Plugin {0}.", f.FullName);
#endif
                            }
                        }
                        catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                    }
                }
                catch { }
                try
                {
                    foreach (Type f in Assembly.GetEntryAssembly().GetTypes())
                    {
                        try
                        {
                            if (!CheckType(f))
                            {
                                //Console.WriteLine("Loaded type {0}.", f.ToString());
                                object type = Activator.CreateInstance(f);
                                Plugins.Push(type);
#if DEBUG
                                Console.WriteLine("Loaded Plugin {0}.", f.FullName);
#endif
                            }
                        }
                        catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                    }
                }
                catch { }
                //load plugins from disk?
                if (LoadPluginsFromDisk)
                {
                    string PluginPath = Path.Combine(Environment.CurrentDirectory, "Plugins");
                    if (!Directory.Exists(PluginPath)) { Directory.CreateDirectory(PluginPath); }
                    foreach (string filename in Directory.GetFiles(PluginPath))
                    {
                        if (filename.EndsWith(".dll"))
                        {
                            //Initialize unpacker.
                            Assembly g = Assembly.Load(File.ReadAllBytes(filename));
                            //Test to see if it's a module.
                            if (g != null)
                            {
                                foreach (Type f in g.GetTypes())
                                {
                                    if (!CheckType(f))
                                    {
                                        object plug = Activator.CreateInstance(f);
                                        Plugins.Push(plug);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException e)
            {
                foreach (Exception p in e.LoaderExceptions)
                    Console.WriteLine(p.ToString());
            }
            loaded = true;
        }

        public static bool LoadCustomPlugin(Type f)
        {
            try
            {
                if (!CheckType(f))
                {
                    //Console.WriteLine("Loaded type {0}.", f.ToString());
                    object type = Activator.CreateInstance(f);
                    Plugins.Push(type);
#if DEBUG
                    Console.WriteLine("Loaded Plugin {0}.", f.FullName);
#endif
                    return true;
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return false;
        }

        static List<Type> pluginTypes = new List<Type>(){ typeof(LoginHandler), typeof(DataHandler), typeof(ConnectHandler), typeof(ClientConnectedHandler), typeof(ConnectSocketOverrideHandler) };

        private static bool CheckType(Type p)
        {
            foreach(Type x in pluginTypes)
            {
                if (x.IsAssignableFrom(p) && p != x)
                    return false;
                else
                    continue;
            }
            return true;
        }

        static bool loaded = false;

        public static List<object> LoadPlugin(Type assemblytype)
        {
            //make sure plugins are loaded.
            List<object> list = new List<object>();
            foreach (object x in Plugins)
            {
                if (assemblytype.IsAssignableFrom(x.GetType()))
                {
                    if(((GenericPlugin)x).OnStart())
					    if(((GenericPlugin)x).Enabled)
                        	list.Push(x);
                }
            }
            return list;
        }

        public static List<object> GetPlugins
        {
            get { return Plugins; }
        }

        public static void ChangePluginStatus(bool Enabled, Type pluginType)
        {
            foreach (object x in Plugins)
            {
                if(x.GetType() == pluginType)
                {
                    //cast to generic type.
                    ((GenericPlugin)x).Enabled = Enabled;
                    break;
                }
            }
        }
    }
}
