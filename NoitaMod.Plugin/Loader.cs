using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NoitaMod.Log;
using NoitaMod.Util;

namespace NoitaMod.Plugin
{
    public class Loader : Singleton<Loader>, IDisposable
    {
        List<IPlugin> plugins;
        AppDomain appDomain;
        string pluginsFolder;
        List<string> assemblies;

        public void Init()
        {
            Logger.Instance.WriteLine( "NoitaMod.Plugin.Loader.Init()" );

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ReflectionOnlyAssemblyResolve;
            appDomain = AppDomain.CreateDomain( "plugins container" );

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
            }
        }

        public void Dispose()
        {
            plugins.ForEach( plugin =>
            {
                Logger.Instance.WriteLine( $"IPlugin.OnUnload: {plugin.PluginInfo.Name}" );
                plugin.OnUnload();
            } );
            AppDomain.Unload( appDomain );
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
                foreach ( var type in assembly.DefinedTypes )
                {
                    if ( IsValidType( type ) )
                    {
                        Logger.Instance.WriteLine( $"Creating instance of assembly {assembly.GetName()}" );
                        var plugin = appDomain.CreateInstanceFromAndUnwrap(assembly.Location, type.FullName) as IPlugin;
                        plugins.Add( plugin );
                    }
                }
            }

            plugins.ForEach( plugin =>
            {
                Logger.Instance.WriteLine( $"IPlugin.OnLoad: {plugin.PluginInfo.Name}" );
                plugin.OnLoad();
            } );
        }

        static bool IsValidType( TypeInfo type )
        {
            return type.IsClass &&
                !type.IsAbstract &&
                type.ImplementedInterfaces.Any( i => i.GUID == typeof( IPlugin ).GUID &&
                type.BaseType == typeof( MarshalByRefObject ) );
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
