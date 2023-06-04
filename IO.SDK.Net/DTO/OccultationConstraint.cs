// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct OccultationConstraint
{
    public int ObserverId;
    public int BackBodyId;
    public int FrontId;
    public string Type;
    public string AberrationId;
    public double InitialStepSize;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
    public Window[] Windows;

    //Occultation
    public OccultationConstraint(int observerId, int backBodyId, int frontId, string type, string aberrationId,
        double initialStepSize)
    {
        ObserverId = observerId;
        BackBodyId = backBodyId;
        FrontId = frontId;
        Type = type;
        AberrationId = aberrationId;
        InitialStepSize = initialStepSize;
        Windows = new Window[1000];
    }
}