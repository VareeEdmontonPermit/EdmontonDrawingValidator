using System.Collections.Generic;
using System;
using System.Drawing;

namespace EdmontonDrawingValidator.Model
{
    public sealed class DxfLayersName
    {
        /// <summary>
        /// $insunits
        /// </summary>
        public const string ScaleUnit = "$insunits";

        /// <summary>
        /// acdbpolyline
        /// </summary>
        public const string PolyLine = "acdbpolyline";

        /// <summary>
        /// acdbline
        /// </summary>
        public const string Line = "acdbline";

        /// <summary>
        /// acdbentity
        /// </summary>
        public const string Entity = "acdbentity";


        /// <summary>
        /// acdblayertablerecord
        /// </summary>
        public const string TableRecord = "acdblayertablerecord";

        /// <summary>
        /// acdbtext
        /// </summary>
        public const string Text = "acdbtext";

        /// <summary>
        /// acdbcircle
        /// </summary>
        public const string Circle = "acdbcircle";

        /// <summary>
        /// acdbmtext
        /// </summary>
        public const string MText = "acdbmtext";

        /// <summary>
        /// acdbblockreference
        /// </summary>
        public const string AcdbBlockReference = "acdbblockreference";

        /// <summary>
        /// acdbblockbegin
        /// </summary>
        public const string AcdbBlockBegin = "acdbblockbegin";

        /// <summary>
        /// acdbblockend
        /// </summary>
        public const string AcdbBlockEnd = "acdbblockend";

        /// <summary>
        /// endblk
        /// </summary>
        public const string EndBlock = "endblk";

        /// <summary>
        /// "  0"
        /// </summary>
        public const string AcdbBlockEndValue = "  0";

        /// <summary>
        /// _lot
        /// </summary>
        public const string Lot = "_lot";

        /// <summary>
        /// _mechroom
        /// </summary>
        public const string MechRoom = "_mechroom";

        /// <summary>
        /// _farspace
        /// </summary>
        public const string FARSpace = "_farspace";

        /// <summary>
        /// _treeboulevard
        /// </summary>
        public const string Treeboulevard = "_treeboulevard";

        /// <summary>
        /// _floorplan
        /// </summary>
        public const string Floorplan = "_floorplan";

        /// <summary>
        /// _floorjoist
        /// </summary>
        public const string Floorjoist = "_floorjoist";

        /// <summary>
        /// _driveway
        /// </summary>
        public const string Driveway = "_driveway";

        /// <summary>
        /// _side2Elevationplan
        /// </summary>
        public const string Side2Elevationplan = "_side2elevationplan";

        /// <summary>
        /// _side1Elevationplan
        /// </summary>
        public const string Side1Elevationplan = "_side1elevationplan";

        /// <summary>
        /// _RearElevationplan
        /// </summary>
        public const string RearElevationplan = "_rearelevationplan";

        /// <summary>
        /// _frontelevationplan
        /// </summary>
        public const string FrontElevationplan = "_frontelevationplan";

        /// <summary>
        /// _sectionplan
        /// </summary>
        public const string Sectionplan = "_sectionplan";

        

        /// <summary>
        /// _rearmargin
        /// </summary>
        public const string RearMargin = "_rearmarginline";

        /// <summary>
        /// _frontroad
        /// </summary>
        public const string FrontRoad = "_frontroad";

        /// <summary>
        /// _otherroad
        /// </summary>
        public const string OtherRoad = "_otherroad";

        /// <summary>
        /// _garage
        /// </summary>
        public const string Garage = "_garage";

        /// <summary>
        /// _abline
        /// </summary>
        public const string ABLine = "_abline";
        
        /// <summary>
        /// _rearmarginline
        /// </summary>
        public const string RearMarginLine = "_rearmarginline";

        /// <summary>
        /// _rearmarginline
        /// </summary>
        public const string SectionPlan = "_sectionplan";

        /// <summary>
        /// _unit
        /// </summary>
        public const string Unit = "_unit";

        /// <summary>
        /// _unit
        /// </summary>
        public const string Alley = "_alley";

        /// <summary>
        /// _gradeline
        /// </summary>
        public const string GradeLine = "_gradeline";

        /// <summary>
        /// _floorjoist
        /// </summary>
        public const string FloorJoist = "_floorjoist";

        /// <summary>
        /// _roofline
        /// </summary>
        public const string RoofLine = "_roofline";

        /// <summary>
        /// _topofwallplate
        /// </summary>
        public const string TopOfWallPlate = "_topofwallplate";
         
        /// <summary>
        /// _floorline
        /// </summary>
        public const string FloorLine = "_floorline";


        /// <summary>
        /// _errorvalidatemessage
        /// </summary>
        public const string ValidateErrorMessage = "_errorvalidatemessage";

        /// <summary>
        /// _accessoryuse
        /// </summary>
        public const string AccessoryUse = "_accessoryuse";

        /// <summary>
        /// _archproj
        /// </summary>
        public const string ArchProject = "_archproj";

        /// <summary>
        /// _atrium
        /// </summary>
        public const string Atrium = "_atrium";

        /// <summary>
        /// _beam
        /// </summary>
        public const string Beam = "_beam";

        /// <summary>
        /// _building
        /// </summary>
        public const string Building = "_building";

        /// <summary>
        /// _refugeearea
        /// </summary>
        public const string RefugeeArea = "_refugeearea";


        /// <summary>
        /// _chaja
        /// </summary>
        public const string Chaja = "_chaja";

        ///// <summary>
        ///// _commfsi
        ///// </summary>
        //public const string CommercialFSI = "_commfsi";

        /// <summary>
        /// _commbuiltupline
        /// </summary>
        public const string CommercialBuiltUpLine = "_commbuiltupline";

        /// <summary>
        /// _commonplot
        /// </summary>
        public const string CommonPlot = "_commonplot";

        /// <summary>
        /// _driveway
        /// </summary>
        public const string DriveWay = "_driveway";

        ///// <summary>
        ///// _indfsi
        ///// </summary>
        //public const string IndustrialFSI = "_indfsi";

        /// <summary>
        /// _indbuiltupline
        /// </summary>
        public const string IndustrialBuiltUpLine = "_indbuiltupline";
        //public const string SpecialFSI = "_specialfsi";
        //public const string SpecialBuiltUpLine = "_specialbuiltupline";

        /// <summary>
        /// _door
        /// </summary>
        public const string Door = "_door";

        /// <summary>
        /// _exstructure
        /// </summary>
        public const string Exstructure = "_exstructure";


        /// <summary>
        /// _exbuiltupline
        /// </summary>
        public const string ExBuiltUpLine = "_exbuiltupline";

        /// <summary>
        /// _floor
        /// </summary>
        public const string Floor = "_floor";

        /// <summary>
        /// _floorinsection
        /// </summary>
        public const string FloorInSection = "_floorinsection";

        /// <summary>
        /// _floorinsection
        /// </summary>
        public const string WithInMarginLine = "_withinmarginline";

        /// <summary>
        /// _groundlevel
        /// </summary>
        public const string GroundLevel = "_groundlevel";

        /// <summary>
        /// _hfl
        /// </summary>
        public const string HighFloodLevel = "_hfl";

        /// <summary>
        /// _basementarea
        /// </summary>
        public const string BasementArea = "_basementarea";

        /// <summary>
        /// _lift
        /// </summary>
        public const string Lift = "_lift";

        /// <summary>
        /// _mainroad
        /// </summary>
        public const string MainRoad = "_mainroad";

        /// <summary>
        /// _amalgamation
        /// </summary>
        public const string Amalgamation = "_amalgamation";

        /// <summary>
        /// _tree
        /// </summary>
        public const string Tree = "_tree";

        /// <summary>
        /// _marginline
        /// </summary>
        public const string MarginLine = "_marginline";

        ///// <summary>
        ///// _specialusefsi
        ///// </summary>
        //public const string SpecialUseFSI = "_specialusefsi";

        /// <summary>
        /// _specialusebuiltupline
        /// </summary>
        public const string SpecialUseBuiltUpLine = "_specialusebuiltupline";

        /// <summary>
        /// _netplot
        /// </summary>
        public const string NetPlot = "_netplot";

        /// <summary>
        /// _commonreferencepoint
        /// </summary>
        public const string CommonReferencePoint = "_commonreferencepoint";

        /// <summary>
        /// _ots
        /// </summary>
        public const string OTS = "_ots";

        ///// <summary>
        ///// _owt
        ///// </summary>
        //public const string OWT = "_owt";

        /// <summary>
        /// _parking
        /// </summary>
        public const string Parking = "_parking";

        /// <summary>
        /// _plot
        /// </summary>
        public const string Plot = "_plot";

        /// <summary>
        /// _indivsubplot
        /// </summary>
        public const string IndividualSubPlot = "_indivsubplot";

        /// <summary>
        /// _subdivision
        /// </summary>
        public const string Subdivision = "_subdivision";

        ///// <summary>
        ///// _propose
        ///// </summary>
        //public const string ProposedWork = "_propose";

        /// <summary>
        /// _propwork
        /// </summary>
        public const string Propwork = "_propwork";

        /// <summary>
        /// _ramp
        /// </summary>
        public const string Ramp = "_ramp";

        ///// <summary>
        ///// _resifsi
        ///// </summary>
        //public const string ResidentFSI = "_resifsi";

        /// <summary>
        /// _resibuiltupline
        /// </summary>
        public const string ResidentBuiltUpLine = "_resibuiltupline";

        /// <summary>
        /// _room
        /// </summary>
        public const string Room = "_room";

        /// <summary>
        /// _staircase
        /// </summary>
        public const string StairCase = "_staircase";

        /// <summary>
        /// _section
        /// </summary>
        public const string Section = "_section";

        /// <summary>
        /// _sectionalitem
        /// </summary>
        public const string SectionalItem = "_sectionalitem";

        /// <summary>
        /// _siteplan
        /// </summary>
        public const string SitePlan = "_siteplan";

        /// <summary>
        /// _unitbua
        /// </summary>
        public const string UnitBUA = "_unitbua";

        /// <summary>
        /// _void
        /// </summary>
        public const string Void = "_void";

        /// <summary>
        /// _wall
        /// </summary>
        public const string Wall = "_wall";

        /// <summary>
        /// _internalroad
        /// </summary>
        public const string ResidentInternalRoad = "_internalroad-r";

        /// <summary>
        /// _internalroad
        /// </summary>
        public const string NonResidentInternalRoad = "_internalroad-nr";


        /// <summary>
        /// _passage
        /// </summary>
        public const string Passage = "_passage";

        /// <summary>
        /// _roadwidening
        /// </summary>
        public const string RoadWidening = "_roadwidening";

        /// <summary>
        /// _balcony
        /// </summary>
        public const string Balcony = "_balcony";

        /// <summary>
        /// _terrace
        /// </summary>
        public const string Terrace = "_terrace";

        /// <summary>
        /// _otherdetail
        /// </summary>
        public const string OtherDetail = "_otherdetail";

        /// <summary>
        /// _window
        /// </summary>
        public const string Window = "_window";

        /// <summary>
        /// _Sanitation
        /// </summary>
        public const string Sanitation = "_sanitation";

        /// <summary>
        /// _RoadCurvature
        /// </summary>
        public const string RoadCurvature = "_roadcurvature";

        ///<summary>
        ///_PrintArea
        ///</summary>
        public const string PrintArea = "_printarea";

        /// <summary>
        /// _north
        /// </summary>
        public const string North = "_north";

        /// <summary>
        /// _loft
        /// </summary>
        public const string Loft = "_loft";

        /// <summary>
        /// _waterbody
        /// </summary>
        public const string WaterBody = "_waterbody";

        /// <summary>
        /// _NonOwnerPlot
        /// </summary>
        public const string NonOwnerPlot = "_nonownerplot";

        /// <summary>
        /// _Connector
        /// </summary>
        public const string Connector = "_connector";

        /// <summary>
        /// _skywalk
        /// </summary>
        public const string SkyWalk = "_skywalk";

        /// <summary>
        /// DASHED
        /// </summary>
        public const string DashedLine = "DASHED"; //Dashed


        /// <summary>
        /// DASHED
        /// </summary>
        public const string LiftMachineRoomLine = "DASHED"; //Dashed

        /// <summary>
        /// CENTER
        /// </summary>
        public const string CenterLineCode = "CENTER";

        /// <summary>
        /// CENTER2
        /// </summary>
        public const string CenterLineCode2 = "CENTER2";

        /// <summary>
        /// beam
        /// </summary>
        public const string BeamText = "beam";

        /// <summary>
        /// 8
        /// </summary>
        public const string EntityStartCodeValue = "  8";

        /// <summary>
        /// 8
        /// </summary>
        public const string EntityStartCode = "8";  // "  8"


        /// <summary>
        /// Passenger lift colour code 114
        /// </summary>
        public const string PassengerLiftColour = "114";

        /// <summary>
        /// Fire lift colour code 110
        /// </summary>
        public const string FireLiftColour = "110";

        /// <summary>
        /// Lift lobby colour code 112
        /// </summary>
        public const string LiftLobbyColour = "112";

        /// <summary>
        /// Ex Lift colour code 170
        /// </summary>
        public const string ExLiftColour = "170";

        /// <summary>
        /// 0
        /// </summary>
        public const string EntityEndCode = "0";

        /// <summary>
        /// 1
        /// </summary>
        public const string LayerTextCode = "1";

        /// <summary>
        ///   1
        /// </summary>
        public const string LayerTextCodeValue = "  1";

        /// <summary>
        ///   1
        /// </summary>
        public const string LayerTextCode3Value = "  3";


        /// <summary>
        /// 2
        /// </summary>
        public const string BlockNameCodeValue = "  2";

        /// <summary>
        /// 100
        /// </summary>
        public const string CommandNameCode = "100";

        /// <summary>
        /// 90
        /// </summary>
        public const string LayerNoOfVertices = "90";

        /// <summary>
        /// _
        /// </summary>
        public const string LayerStartWithChar = "_";

        /// <summary>
        /// 6
        /// </summary>
        public const string LineTypeCode = "6";

        /// <summary>
        /// 6
        /// </summary>
        public const string LineTypeCodeValue = "  6";

        /// <summary>
        /// 62        
        /// 62  Color number(if negative, layer is off) Layer colour code if negative then it is 
        /// </summary>
        public const string LineColourCode = "62";

        /// <summary>
        /// 62  Color number(if negative, layer is off) Layer colour code if negative then it is 
        /// </summary>
        public const string LineColourCodeValue1 = " 62";

        /// <summary>
        /// 62  Color number(if negative, layer is off) Layer colour code if negative then it is 
        /// </summary>
        public const string LineColourCodeValue2 = "  62";

        ///70 
        ///        Standard flags(bit-coded values): 
        ///1 = Layer is frozen; otherwise layer is thawed 
        ///2 = Layer is frozen by default in new viewports 
        ///4 = Layer is locked  
        ///16 = If set, table entry is externally dependent on an xref 
        ///32 = If both this bit and bit 16 are set, the externally dependent xref has been successfully resolved 
        ///64 = If set, the table entry was referenced by at least one entity in the drawing the last time the drawing was edited. (This flag is for the benefit of AutoCAD commands.It can be ignored by most programs that read DXF files and need not be set by programs that write DXF files) 

        /// <summary>
        /// 70     Standard flags(bit-coded values): 
        /// 1 = Layer is frozen; otherwise layer is thawed 
        /// 2 = Layer is frozen by default in new viewports 
        /// 4 = Layer is locked  
        /// </summary>
        public const string StandardFlagsCode = "70";

        /// <summary>
        /// 70     Standard flags(bit-coded values): 
        /// 1 = Layer is frozen; otherwise layer is thawed 
        /// 2 = Layer is frozen by default in new viewports 
        /// 4 = Layer is locked  
        /// </summary>
        public const string StandardFlagsCodeValue = " 70";

        /// <summary>
        /// 100
        /// </summary>
        public const string LayerCommandCode = "100";

        /// <summary>
        /// 11
        /// </summary>
        public const string LayerLineX_ValueCode = " 11";

        /// <summary>
        /// 21
        /// </summary>
        public const string LayerLineY_ValueCode = " 21";

        /// <summary>
        /// 11
        /// </summary>
        public const string LayerTextAlignX_ValueCode = " 11";

        /// <summary>
        /// 21
        /// </summary>
        public const string LayerTextAlignY_ValueCode = " 21";

        /// <summary>
        /// 10
        /// </summary>
        public const string LayerPolyLineX_ValueCode = " 10";

        /// <summary>
        /// 20
        /// </summary>
        public const string LayerPolyLineY_ValueCode = " 20";

        /// <summary>
        /// 50
        /// </summary>
        public const string ReferenceRotationAngle_ValueCode = " 50";

        /// <summary>
        /// 41
        /// </summary>
        public const string XScaling_ValueCode = " 41";

        /// <summary>
        /// 42
        /// </summary>
        public const string YScaling_ValueCode = " 42";

        /// <summary>
        /// 43
        /// </summary>
        public const string ZScaling_ValueCode = " 43";


        /// <summary>
        /// 42
        /// </summary>
        public const string LayerPolyLineBulge_ValueCode = "42";

        /// <summary>
        /// 40
        /// </summary>
        public const string CircleRadius_ValueCode = "40";

        /// <summary>
        /// 51
        /// </summary>
        public const string CircleRadius_StartAngle = "50";

        /// <summary>
        /// 51
        /// </summary>
        public const string CircleRadius_EndAngle = "51";

        /// <summary>
        /// 6
        /// </summary>
        public const string RearMarginLineColour = "6"; //pink

        /// <summary>
        /// 1
        /// </summary>
        public const string FrontMarginLineColour = "1"; //red

        /// <summary>
        /// 5
        /// </summary>
        public const string Side1MarginLineColour = "5"; //blue

        /// <summary>
        /// 104
        /// </summary>
        public const string Side2MarginLineColour = "104"; //gray

        /// <summary>
        /// 63
        /// </summary>
        public const string TwoStackParkingColour = "63"; //gray

        /// <summary>
        /// 30
        /// </summary>
        public const string RaiseCopParkingColour = "30";

        /// <summary>
        /// 52
        /// </summary>
        public const string RaisedCOPColour = "52";

        /// <summary>
        /// 52
        /// </summary>
        public const string ParkingWithInRaisedCOPColour = "52";


        /// <summary>
        /// stair lobby colour 230
        /// </summary>
        public const string StaircaseLobbyColour = "230";

        /// <summary>
        /// Stair landing colour 161
        /// </summary>
        public const string StairLandingColour = "161";

        /// <summary>
        /// Stair colour 120
        /// </summary>
        public const string StairColour = "120";

        /// <summary>
        /// Existing Stair colour 170
        /// </summary>
        public const string ExStairColour = "170";

        /// <summary>
        /// Periphery wall colour code 224
        /// </summary>
        public const string PeripheryWallColour = "224";

        /// <summary>
        /// Print area signature colour code 224
        /// </summary>
        //public const string PrintAreaSignatureColour = "1";

        /// <summary>
        /// Common red ref point 1 colour code 1
        /// </summary>
        public const string CommonRedRefPoint1ColourCode = "1";

        /// <summary>
        /// Common yellow ref point 2 colour code 2
        /// </summary>
        public const string CommonYellowRefPoint2ColourCode = "2";

        /// <summary>
        /// 1
        /// </summary>
        public const string PrintAreaColourCode = "1";

        /// <summary>
        /// 12
        /// </summary>
        public const string RaisedCOPProposedWorkColour = "12";

    }

}