using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NoitaMod.Log;
using NoitaMod.Util;
using NoitaMod.Common;
using NoitaMod.Memory;

namespace NoitaMod.Plugin
{
    public class Loader : Singleton<Loader>, IDisposable
    {
        List<IPlugin> plugins;
        string pluginsFolder;
        List<string> assemblies;

        public void Init()
        {
            Logger.Instance.WriteLine( "NoitaMod.Plugin.Loader.Init()" );

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ReflectionOnlyAssemblyResolve;

            pluginsFolder = Directory.GetCurrentDirectory() + @"\NoitaMod\plugins";

            if ( !Directory.Exists( pluginsFolder ) )
            {
                Directory.CreateDirectory( pluginsFolder );
            }

            try
            {
                LoadPlugins();
            }
            catch ( ReflectionTypeLoadException ex )
            {
                foreach ( Exception exSub in ex.LoaderExceptions )
                {
                    Logger.Instance.WriteLine( exSub.Message, LogLevel.Error );
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if ( exFileNotFound != null )
                    {
                        if ( !string.IsNullOrEmpty( exFileNotFound.FusionLog ) )
                        {
                            Logger.Instance.WriteLine( exFileNotFound.FusionLog );
                        }
                    }
                }

                throw ex;
            }
        }

        public void Dispose()
        {
            plugins.ForEach( plugin =>
            {
                Logger.Instance.WriteLine( $"IPlugin.OnUnload: {plugin.PluginInfo.Name}" );
                plugin.OnUnload();
            } );
        }

        void LoadPlugins()
        {
            assemblies = Directory.GetFiles( pluginsFolder, "*.dll" ).ToList();
            plugins = new List<IPlugin>();

            if ( assemblies.Count > 0 )
            {
                Logger.Instance.WriteLine( "Plugin files:" );
                assemblies.ForEach( a => Logger.Instance.WriteLine( $"  {a}" ) );
            }
            else
            {
                Logger.Instance.WriteLine( "No plugins found" );
            }

            foreach ( var assembly in assemblies.Select( Assembly.ReflectionOnlyLoadFrom ) )
            {
                var valid = false;

                foreach ( var type in assembly.DefinedTypes )
                {
                    if ( IsValidType( type ) )
                    {
                        valid = true;
                        Logger.Instance.WriteLine( $"Creating instance of assembly {assembly.GetName()}" );
                        var plugin = AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(assembly.Location, type.FullName) as IPlugin;
                        plugins.Add( plugin );
                    }
                }

                if ( !valid )
                {
                    Logger.Instance.WriteLine( $"No valid IPlugin implementation found | {assembly.GetName()}", LogLevel.Error );
                }
            }

            Host host = new Host()
            {
                Logger = Logger.Instance,
                Scanner = Scanner.Instance
            };

            plugins.ForEach( plugin =>
            {
                Logger.Instance.WriteLine( $"IPlugin.OnLoad(): {plugin.PluginInfo.Name}" );
                plugin.OnLoad( host );
            } );

            if ( plugins.Count == 0 )
            {
                Logger.Instance.WriteLine( "No plugin instances" );
            }
        }

        static bool IsValidType( TypeInfo type )
        {
            return type.IsClass &&
                !type.IsAbstract &&
                type.ImplementedInterfaces.Any( i => i.GUID == typeof( IPlugin ).GUID );
        }

        private static Assembly ReflectionOnlyAssemblyResolve( object sender, ResolveEventArgs args )
        {
            var strTempAssmbPath = "";

            var objExecutingAssemblies = Assembly.GetExecutingAssembly();
            var arrReferencedAssmbNames = objExecutingAssemblies.GetReferencedAssemblies();

            var match = arrReferencedAssmbNames.Any( strAssmbName =>
                strAssmbName.FullName.Substring( 0, strAssmbName.FullName.IndexOf( ",", StringComparison.Ordinal ) ) ==
                args.Name.Substring( 0, args.Name.IndexOf( ",", StringComparison.Ordinal ) )
            );

            if ( match )
            {
                strTempAssmbPath = Path.GetDirectoryName( objExecutingAssemblies.Location ) + "\\" +
                    args.Name.Substring( 0, args.Name.IndexOf( ",", StringComparison.Ordinal ) ) + ".dll";
            }

            return Assembly.ReflectionOnlyLoadFrom( strTempAssmbPath );
        }
    }
}
