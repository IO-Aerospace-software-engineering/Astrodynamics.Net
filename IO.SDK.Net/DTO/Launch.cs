// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Launch
{
    public Window Window;
    public Site LaunchSite;
    public Site RecoverySite;
    public bool LaunchByDay;
    public double InitialStepSize;
    public StateVector TargetOrbit;
    public double InertialAzimuth;
    public double NonInertialAzimuth;

    public double NonInertialInsertionVelocity;
    public double InertialInsertionVelocity;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public Window[] Windows;

    public Launch(Site launchSite, Site recoverySite, bool launchByDay, double initialStepSize, StateVector targetOrbit,
        Window window)
    {
        Windows = new Window[100];
        Window = window;
        LaunchSite = launchSite;
        RecoverySite = recoverySite;
        LaunchByDay = launchByDay;
        InitialStepSize = initialStepSize;
        TargetOrbit = targetOrbit;
        InertialAzimuth = default;
        InertialInsertionVelocity = default;
        NonInertialAzimuth = default;
        NonInertialInsertionVelocity = default;
    }
}