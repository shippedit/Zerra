﻿// Copyright © KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Zerra
{
    public static class Config
    {
        private const string settingsFileName = "appsettings.json";
        private const string genericSettingsFileName = "appsettings.{0}.json";

        private const string environmentNameVariable1 = "ASPNETCORE_ENVIRONMENT";
        private const string environmentNameVariable2 = "Hosting:Environment";
        private const string environmentNameVariable3 = "ASPNET_ENV";

        private const string internalUrl1 = "urls";
        private const string internalUrl2 = "ASPNETCORE_URLS";
        private const string internalUrl3 = "ASPNETCORE_SERVER.URLS";
        private const string internalUrl4 = "DOTNET_URLS"; //docker
        private const string internalUrlDefault = "http://localhost:5000;https://localhost:5001";

        private const string azureSiteName = "WEBSITE_SITE_NAME";
        private const string azureSiteUrls = "http://{0}:80;https://{0}:443";

        private static readonly object discoveryLock = new();
        private static bool discoveryStarted;
        internal static bool DiscoveryStarted
        {
            get
            {
                lock (discoveryLock)
                {
                    return discoveryStarted;
                }
            }
            set
            {
                lock (discoveryLock)
                {
                    discoveryStarted = value;
                }
            }
        }

        internal static string[] DiscoveryNamespaces;

        private static readonly Assembly entryAssembly;
        private static readonly Assembly executingAssembly;
        private static readonly string entryAssemblyName;
        private static readonly string entryNameSpace;
        private static readonly string frameworkNameSpace;
        static Config()
        {
            entryAssembly = Assembly.GetEntryAssembly();
            executingAssembly = Assembly.GetExecutingAssembly();

            entryAssemblyName = entryAssembly?.GetName().Name;

            entryNameSpace = entryAssemblyName?.Split('.')[0] + '.';
            frameworkNameSpace = executingAssembly.GetName().Name.Split('.')[0] + '.';

            DiscoveryNamespaces = entryNameSpace != null ? (new string[] { entryNameSpace, frameworkNameSpace }) : (new string[] { frameworkNameSpace });
            discoveryStarted = false;
        }

        private static IConfiguration configuration = null;
        private static string environmentName = null;
        public static void LoadConfiguration() { LoadConfiguration(null, null, null, null); }
        public static void LoadConfiguration(string environmentName) { LoadConfiguration(null, environmentName, null, null); }
        public static void LoadConfiguration(string[] args) { LoadConfiguration(args, null, null, null); }
        public static void LoadConfiguration(string[] args, string environmentName) { LoadConfiguration(args, environmentName, null, null); }
        public static void LoadConfiguration(string[] args, Action<ConfigurationBuilder> build) { LoadConfiguration(args, null, null, build); }
        public static void LoadConfiguration(Action<ConfigurationBuilder> build) { LoadConfiguration(null, null, null, build); }
        public static void LoadConfiguration(string[] args, string environmentName, Action<ConfigurationBuilder> build) { LoadConfiguration(args, environmentName, null, build); }
        public static void LoadConfiguration(string[] args, string[] settingsFiles) { LoadConfiguration(args, null, settingsFiles, null); }
        public static void LoadConfiguration(string[] args, string environmentName, string[] settingsFiles) { LoadConfiguration(args, environmentName, settingsFiles, null); }
        public static void LoadConfiguration(string[] args, string environmentName, string[] settingsFiles, Action<ConfigurationBuilder> build)
        {
            var builder = new ConfigurationBuilder();

            var settingsFileNames = GetEnvironmentFilesBySuffix(settingsFileName);
            foreach (var settingsFileName in settingsFileNames)
                AddSettingsFile(builder, settingsFileName);

            if (String.IsNullOrWhiteSpace(environmentName))
                environmentName = Environment.GetEnvironmentVariable(environmentNameVariable1);
            if (String.IsNullOrWhiteSpace(environmentName))
                environmentName = Environment.GetEnvironmentVariable(environmentNameVariable2);
            if (String.IsNullOrWhiteSpace(environmentName))
                environmentName = Environment.GetEnvironmentVariable(environmentNameVariable3);

            if (!String.IsNullOrWhiteSpace(environmentName))
            {
                var environmentSettingsFileNames = GetEnvironmentFilesBySuffix(String.Format(genericSettingsFileName, environmentName));
                foreach (var environmentSettingsFileName in environmentSettingsFileNames)
                    AddSettingsFile(builder, environmentSettingsFileName);
            }

            Config.environmentName = environmentName;
            Console.WriteLine($"Environment: {environmentName}");

            if (settingsFiles != null && settingsFiles.Length > 0)
            {
                foreach (var settingsFile in settingsFiles)
                {
                    var file = settingsFile;
                    while (file.StartsWith("/") || file.StartsWith("\\"))
                        file = settingsFile.Substring(1);
                    AddSettingsFile(builder, file);
                }
            }

            _ = builder.AddEnvironmentVariables();

            if (args != null && args.Length > 0)
                _ = builder.AddCommandLine(args);
            
            build?.Invoke(builder);

            configuration = builder.Build();
        }
        public static void SetConfiguration(IConfiguration configuration)
        {
            Config.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private static void AddSettingsFile(ConfigurationBuilder builder, string fileName)
        {
            var file = File.OpenRead(fileName);
            _ = builder.AddJsonStream(file);
            Console.WriteLine($"{nameof(Config)} Loaded {Path.GetFileName(fileName)}");
        }

        public static string GetSetting(string name, params string[] sections)
        {
            if (configuration == null)
                throw new Exception("Config not loaded");

            var config = configuration;
            if (sections != null && sections.Length > 0)
            {
                foreach (var section in sections)
                {
                    config = config.GetSection(section);
                    if (config == null)
                        throw new Exception($"Config section {section} not found");
                }
            }
            var value = config[name];
            return value;
        }

        public static string GetInternalUrl(string defaultUrl)
        {
            var url = GetSetting(internalUrl1);
            if (String.IsNullOrWhiteSpace(url))
                url = GetSetting(internalUrl2);
            if (String.IsNullOrWhiteSpace(url))
                url = GetSetting(internalUrl3);
            if (String.IsNullOrWhiteSpace(url))
                url = GetSetting(internalUrl4);
            if (String.IsNullOrWhiteSpace(url))
                url = defaultUrl;
            if (String.IsNullOrWhiteSpace(url))
            {
                var siteName = GetSetting(azureSiteName);
                if (!String.IsNullOrWhiteSpace(siteName))
                    url = String.Format(azureSiteUrls, siteName);
            }
            if (String.IsNullOrWhiteSpace(url))
                url = internalUrlDefault;
            return url;
        }
        public static string GetExternalUrl(string settingName, string defaultUrl)
        {
            var url = GetSetting(settingName);
            if (String.IsNullOrWhiteSpace(url))
                url = defaultUrl;
            if (String.IsNullOrWhiteSpace(url))
                url = null;
            return url;
        }

        public static IConfiguration Configuration
        {
            get
            {
                if (configuration == null)
                    throw new Exception("Config not loaded");
                return configuration;
            }
        }
        public static string EnvironmentName
        {
            get
            {
                return environmentName;
            }
        }

        public static T Bind<T>(params string[] sections)
        {
            if (configuration == null)
                throw new Exception("Config not loaded");

            var config = configuration;
            if (sections != null && sections.Length > 0)
            {
                foreach (var section in sections)
                {
                    config = config.GetSection(section);
                    if (config == null)
                        throw new Exception($"Config section {section} not found");
                }
            }
            var value = config.Get<T>();
            return value;
        }

        public static string GetEnvironmentFilePath(string fileName)
        {
            var executingAssemblyPath = Path.GetDirectoryName(executingAssembly.Location);
            var filePath = $"{executingAssemblyPath}/{fileName}";
            if (File.Exists(filePath))
                return filePath;

            filePath = $"{Environment.CurrentDirectory}/{fileName}";
            return File.Exists(filePath) ? filePath : null;
        }
        public static IReadOnlyCollection<string> GetEnvironmentFilesBySuffix(string fileSuffix)
        {
            var files = new List<string>();

            var searchPattern = $"*{fileSuffix}";

            var executingAssemblyPath = Path.GetDirectoryName(executingAssembly.Location);
            var executingAssemblyPathFiles = Directory.GetFiles(executingAssemblyPath, searchPattern);
            files.AddRange(executingAssemblyPathFiles);

            if (Environment.CurrentDirectory != executingAssemblyPath)
            {
                var currentDirectoryFiles = Directory.GetFiles(Environment.CurrentDirectory, searchPattern);
                files.AddRange(currentDirectoryFiles);
            }

            return files;
        }

        public static void AddDiscoveryNamespaces(params string[] namespaces)
        {
            lock (discoveryLock)
            {
                if (discoveryStarted)
                    throw new InvalidOperationException("Discovery has already started");

                var newNamespaces = namespaces.Select(x => x + '.').ToArray();

                var newNamespacesToLoad = new string[DiscoveryNamespaces.Length + newNamespaces.Length];
                DiscoveryNamespaces.CopyTo(newNamespacesToLoad, 0);
                newNamespaces.CopyTo(newNamespacesToLoad, DiscoveryNamespaces.Length);
                DiscoveryNamespaces = newNamespacesToLoad;
            }
        }
        public static void AddDiscoveryAssemblies(params Assembly[] assemblies)
        {
            lock (discoveryLock)
            {
                if (discoveryStarted)
                    throw new InvalidOperationException("Discovery has already started");

                var newNamespaces = assemblies.Select(x => x.GetName().Name).ToArray();

                var newNamespacesToLoad = new string[DiscoveryNamespaces.Length + newNamespaces.Length];
                DiscoveryNamespaces.CopyTo(newNamespacesToLoad, 0);
                newNamespaces.CopyTo(newNamespacesToLoad, DiscoveryNamespaces.Length);
                DiscoveryNamespaces = newNamespacesToLoad;
            }
        }

        public static void SetDiscoveryNamespaces(params string[] namespaces)
        {
            lock (discoveryLock)
            {
                if (discoveryStarted)
                    throw new InvalidOperationException("Discovery has already started");

                var newNamespaces = namespaces.Select(x => x + '.').ToArray();

                var newNamespacesToLoad = new string[newNamespaces.Length + 1];
                newNamespacesToLoad[0] = frameworkNameSpace;
                newNamespaces.CopyTo(newNamespacesToLoad, 1);
                DiscoveryNamespaces = newNamespacesToLoad;
            }
        }
        public static void SetDiscoveryAssemblies(params Assembly[] assemblies)
        {
            lock (discoveryLock)
            {
                if (discoveryStarted)
                    throw new InvalidOperationException("Discovery has already started");

                var newNamespaces = assemblies.Select(x => x.GetName().Name).ToArray();

                var newNamespacesToLoad = new string[newNamespaces.Length + 1];
                newNamespacesToLoad[0] = frameworkNameSpace;
                newNamespaces.CopyTo(newNamespacesToLoad, 1);
                DiscoveryNamespaces = newNamespacesToLoad;
            }
        }

        public static string EntryAssemblyName { get { return entryAssemblyName; } }
        public static Assembly EntryAssembly { get { return entryAssembly; } }

        private static readonly Lazy<bool> isDebugEntryAssembly = new(() => entryAssembly.GetCustomAttributes(false).OfType<DebuggableAttribute>().Any(x => x.IsJITTrackingEnabled));
        public static bool IsDebugBuild { get { return isDebugEntryAssembly.Value; } }
    }
}