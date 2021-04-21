﻿#if !NOT_DI
using Microsoft.Extensions.DependencyInjection;
#endif
using System.Linq;
using System.Runtime.InteropServices;
#if !NOT_XE
using Xamarin.Essentials;
#endif

namespace System
{
    /// <summary>
    /// 依赖注入服务组(DependencyInjection)
    /// </summary>
    public static class DI
    {
#if !NOT_DI

        static IServiceProvider? value;

        internal static bool IsInit => value != null;

        internal static IServiceProvider Value => value ?? throw new NullReferenceException("init must be called.");

        /// <summary>
        /// 初始化依赖注入服务组(通过配置服务项的方式)
        /// </summary>
        /// <param name="configureServices"></param>
        public static void Init(Action<IServiceCollection> configureServices)
        {
            var services = new ServiceCollection();
            configureServices(services);
            Init(services.BuildServiceProvider());
        }

        /// <summary>
        /// 初始化依赖注入服务组(直接赋值)
        /// </summary>
        /// <param name="serviceProvider"></param>
        public static void Init(IServiceProvider serviceProvider)
        {
            value = serviceProvider;
        }

#endif

        /// <inheritdoc cref="System.Platform"/>
        public static Platform Platform { get; }

        /// <inheritdoc cref="System.DeviceIdiom"/>
        public static DeviceIdiom DeviceIdiom { get; private set; }

        /// <summary>
        /// 当前是否使用Mono运行时
        /// </summary>
        public static bool IsRunningOnMono { get; }

        /// <summary>
        /// 当前进程运行的构架为 <see cref="Architecture.X86"/> 或 <see cref="Architecture.X64"/>
        /// </summary>
        public static bool IsX86OrX64 { get; }

        /// <summary>
        /// 当前进程运行的构架为 <see cref="Architecture.Arm64"/>
        /// </summary>
        public static bool IsArm64 { get; }

        const string DesktopWindowTypeNames =
            "Avalonia.Controls.Window, Avalonia.Controls" +
            "\n" +
            "System.Windows.Forms.Form, System.Windows.Forms" +
            "\n" +
            "System.Windows.Window, PresentationFramework";

        static DI()
        {
            IsRunningOnMono = Type.GetType("Mono.Runtime") != null;
            var processArchitecture = RuntimeInformation.ProcessArchitecture;
            IsX86OrX64 = processArchitecture == Architecture.X64 || processArchitecture == Architecture.X86;
            IsArm64 = processArchitecture == Architecture.Arm64;
            static Platform RuntimeInformationOSPlatform()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return Platform.Windows;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return Platform.Apple;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return Platform.Linux;
                return Platform.Unknown;
            }
            static DeviceIdiom GetDeviceIdiom()
            {
                if (DesktopWindowTypeNames.Split('\n')
                    .Any(x => Type.GetType(x) != null))
                    return DeviceIdiom.Desktop;
                return DeviceIdiom.Unknown;
            }
#if !NOT_XE
            Platform = DeviceInfo.Platform.Convert();
            if (Platform == Platform.Unknown) Platform = RuntimeInformationOSPlatform();
            DeviceIdiom = DeviceInfo.Idiom.Convert();
            if (DeviceIdiom == DeviceIdiom.Unknown) DeviceIdiom = GetDeviceIdiom();
#else
            Platform = RuntimeInformationOSPlatform();
            DeviceIdiom = GetDeviceIdiom();
#endif
        }

#if !NOT_DI

        static Exception GetDIGetFailException(Exception e, Type serviceType) => new Exception($"DI.Get fail, serviceType: {serviceType}", e);

        /// <summary>
        /// 获取依赖注入服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : notnull
        {
            try
            {
                return Value.GetRequiredService<T>();
            }
            catch (NullReferenceException e)
            {
                throw GetDIGetFailException(e, typeof(T));
            }
        }

        /// <inheritdoc cref="Get{T}"/>
        public static T? Get_Nullable<T>() where T : notnull
        {
            try
            {
                return Value.GetService<T>();
            }
            catch (NullReferenceException e)
            {
                throw GetDIGetFailException(e, typeof(T));
            }
        }

        /// <inheritdoc cref="Get{T}"/>
        public static object Get(Type serviceType)
        {
            try
            {
                return Value.GetRequiredService(serviceType);
            }
            catch (NullReferenceException e)
            {
                throw GetDIGetFailException(e, serviceType);
            }
        }

        /// <inheritdoc cref="Get_Nullable{T}"/>
        public static object? Get_Nullable(Type serviceType)
        {
            try
            {
                return Value.GetService(serviceType);
            }
            catch (NullReferenceException e)
            {
                throw GetDIGetFailException(e, serviceType);
            }
        }

        public static IServiceScope CreateScope() => Value.CreateScope();

#endif
    }
}