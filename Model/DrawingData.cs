using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdmontonDrawingValidator.Model
{
	[Serializable]
    public sealed class DrawingPoint
	{
        [JsonProperty(Order = 1)]
        public double X { get; set; }

        [JsonProperty(Order = 2)]
        public double Y { get; set; }
		public override string ToString()
		{
			return X + "," + Y;
		}
    }
    public sealed class DrawingLine
	{
        [JsonProperty(Order = 1)]
        public DrawingPoint StartPoint { get; set; }

        [JsonProperty(Order = 2)]
        public DrawingPoint EndPoint { get; set; }
		public override string ToString()
		{
			return "(" + StartPoint.ToString() + "),(" + EndPoint.ToString() + ")";
		}
	}
    public sealed class DrawingDataSet
	{
        [JsonProperty(Order = 1)]
        public AutocadColourCode ColourCode { get; set; } = new AutocadColourCode();

        [JsonProperty(Order = 2)]
        public string LineType { get; set; } = "";

        [JsonProperty(Order = 3)]
        public bool IsCircle { get; set; } = false;

        [JsonProperty(Order = 4)]
        public bool HasBulge { get; set; } = false;

        [JsonProperty(Order = 5)]
        public double? Radius { get; set; } = null;

        [JsonProperty(Order = 6)]
        public DrawingPoint CenterPoint { get; set; } = null;

        [JsonProperty(Order = 7)]
        public double? StartAngle { get; set; } = null;

        [JsonProperty(Order = 8)]
        public double? EndAngle { get; set; } = null;

        [JsonProperty(Order = 9)]
        public List<DrawingDataForBulge> PointsWithBulgeValue { get; set; } = null;

        [JsonProperty(Order = 10)]
        public List<DrawingPoint> Points { get; set; } = null;
		//public List<DrawingLine> Lines
  //      {
  //          get
  //          {
		//		List<DrawingLine> lstResult = null;
		//		if(Points != null && Points.Count > 1)
  //              {
  //                  lstResult = new List<DrawingLine>();
  //                  for (int i=1;i<Points.Count;i++)
  //                  {
		//				lstResult.Add(new DrawingLine
		//				{
		//					StartPoint = Points[i - 1],
		//					EndPoint = Points[i]
		//				});
  //                  }
  //              }
		//		return lstResult;
  //          }
		//} 

    }
    public sealed class DrawingDataForBulge
	{
        [JsonProperty(Order = 1)]
        public DrawingPoint StartPoint { get; set; } = null;

        [JsonProperty(Order = 2)]
        public DrawingPoint EndPoint { get; set; } = null;

        [JsonProperty(Order = 3)]
        public double? BulgeValue { get; set; } = null;
	}
    public sealed class DrawingTextData
	{
        [JsonProperty(Order = 1)]
        public DrawingPoint Point { get; set; } = null;

        [JsonProperty(Order = 2)]
        public string Text { get; set; } = "";
	}

    public sealed class DrawingPointMinMax
    {
        [JsonProperty(Order = 1)]
        public double Minimum { get; set; }

        [JsonProperty(Order = 2)]
        public double Maximum { get; set; }
    }

    public sealed class AllDrawingData
    {
		[JsonProperty(Order = 1)]
		public DrawingPointMinMax X { get; set; } = new DrawingPointMinMax();

		[JsonProperty(Order = 2)]
		public DrawingPointMinMax Y { get; set; } = new DrawingPointMinMax();

        [JsonProperty(Order = 3)]

        public List<DrawingData> DrawingData = new List<DrawingData>();
    }
    public sealed class DrawingData
	{
        [JsonProperty(Order = 1)]
        public string LayerName { get; set; } = "";

        [JsonProperty(Order = 2)]
        public DrawingTextData TextInfo { get; set; } = null;

        [JsonProperty(Order = 3)]
        public DrawingDataSet Data { get; set; } = null;

	}

    public sealed class AutocadColourCode
	{
        [JsonProperty(Order = 1)]
        public int? AutocadColourIndex { get; set; } = null;
        
		[JsonProperty(Order = 2)]
        public HexColourCode HexCode { get; set; }

        [JsonProperty(Order = 3)]
        public RGBColourCode RGBCode { get; set; }

	}

    public sealed class HexColourCode
	{
		public string Red { get; set; }
		public string Green { get; set; }
		public string Blue { get; set; }
		public override string ToString()
		{
			return Red + Green + Blue;
		}
	}

    public sealed class RGBColourCode
	{
		public int Red { get; set; } = 0;

		public int Green { get; set; } = 0;

		public int Blue { get; set; } = 0;
		public override string ToString()
		{
			return "(" + Red + "," + Green + "," + Blue + ")";
		}
		public int SumColourCode()
		{
			return Red + Green + Blue;
		}
	}

    public sealed class ColourDictionary
	{
		//Hexadecimal AutoCAD
		//   ColorIndex #	Decimal
		//Red	Green	Blue	Index	Red	Green	Blue

		private const string ColourTableMap = @"
	00	00	00	0	00	00	00
	FF	00	00	1	255	00	00
	FF	FF	00	2	255	255	00
	00	FF	00	3	00	255	00
	00	FF	FF	4	00	255	255
	00	00	FF	5	00	00	255
	FF	00	FF	6	255	00	255
	FF	FF	FF	7	255	255	255
	41	41	41	8	65	65	65
	80	80	80	9	128	128	128
	FF	00	00	10	255	00	00
	FF	AA	AA	11	255	170	170
	BD	00	00	12	189	00	00
	BD	7E	7E	13	189	126	126
	81	00	00	14	129	00	00
	81	56	56	15	129	86	86
	68	00	00	16	104	00	00
	68	45	45	17	104	69	69
	4F	00	00	18	79	00	00
	4F	35	35	19	79	53	53
	FF	3F	00	20	255	63	00
	FF	BF	AA	21	255	191	170
	BD	2E	00	22	189	46	00
	BD	8D	7E	23	189	141	126
	81	1F	00	24	129	31	00
	81	60	56	25	129	96	86
	68	19	00	26	104	25	00
	68	4E	45	27	104	78	69
	4F	13	00	28	79	19	00
	4F	3B	35	29	79	59	53
	FF	7F	00	30	255	127	00
	FF	D4	AA	31	255	212	170
	BD	5E	00	32	189	94	00
	BD	9D	7E	33	189	157	126
	81	40	00	34	129	64	00
	81	6B	56	35	129	107	86
	68	34	00	36	104	52	00
	68	56	45	37	104	86	69
	4F	27	00	38	79	39	00
	4F	42	35	39	79	66	53
	FF	BF	00	40	255	191	00
	FF	EA	AA	41	255	234	170
	BD	8D	00	42	189	141	00
	BD	AD	7E	43	189	173	126
	81	60	00	44	129	96	00
	81	76	56	45	129	118	86
	68	4E	00	46	104	78	00
	68	5F	45	47	104	95	69
	4F	3B	00	48	79	59	00
	4F	49	35	49	79	73	53
	FF	FF	00	50	255	255	00
	FF	FF	AA	51	255	255	170
	BD	BD	00	52	189	189	00
	BD	BD	7E	53	189	189	126
	81	81	00	54	129	129	00
	81	81	56	55	129	129	86
	68	68	00	56	104	104	00
	68	68	45	57	104	104	69
	4F	4F	00	58	79	79	00
	4F	4F	35	59	79	79	53
	BF	FF	00	60	191	255	00
	EA	FF	AA	61	234	255	170
	8D	BD	00	62	141	189	00
	AD	BD	7E	63	173	189	126
	60	81	00	64	96	129	00
	76	81	56	65	118	129	86
	4E	68	00	66	78	104	00
	5F	68	45	67	95	104	69
	3B	4F	00	68	59	79	00
	49	4F	35	69	73	79	53
	7F	FF	00	70	127	255	00
	D4	FF	AA	71	212	255	170
	5E	BD	00	72	94	189	00
	9D	BD	7E	73	157	189	126
	40	81	00	74	64	129	00
	6B	81	56	75	107	129	86
	34	68	00	76	52	104	00
	56	68	45	77	86	104	69
	27	4F	00	78	39	79	00
	42	4F	35	79	66	79	53
	3F	FF	00	80	63	255	00
	BF	FF	AA	81	191	255	170
	2E	BD	00	82	46	189	00
	8D	BD	7E	83	141	189	126
	1F	81	00	84	31	129	00
	60	81	56	85	96	129	86
	19	68	00	86	25	104	00
	4E	68	45	87	78	104	69
	13	4F	00	88	19	79	00
	3B	4F	35	89	59	79	53
	00	FF	00	90	00	255	00
	AA	FF	AA	91	170	255	170
	00	BD	00	92	00	189	00
	7E	BD	7E	93	126	189	126
	00	81	00	94	00	129	00
	56	81	56	95	86	129	86
	00	68	00	96	00	104	00
	45	68	45	97	69	104	69
	00	4F	00	98	00	79	00
	35	4F	35	99	53	79	53
	00	FF	3F	100	00	255	63
	AA	FF	BF	101	170	255	191
	00	BD	2E	102	00	189	46
	7E	BD	8D	103	126	189	141
	00	81	1F	104	00	129	31
	56	81	60	105	86	129	96
	00	68	19	106	00	104	25
	45	68	4E	107	69	104	78
	00	4F	13	108	00	79	19
	35	4F	3B	109	53	79	59
	00	FF	7F	110	00	255	127
	AA	FF	D4	111	170	255	212
	00	BD	5E	112	00	189	94
	7E	BD	9D	113	126	189	157
	00	81	40	114	00	129	64
	56	81	6B	115	86	129	107
	00	68	34	116	00	104	52
	45	68	56	117	69	104	86
	00	4F	27	118	00	79	39
	35	4F	42	119	53	79	66
	00	FF	BF	120	00	255	191
	AA	FF	EA	121	170	255	234
	00	BD	8D	122	00	189	141
	7E	BD	AD	123	126	189	173
	00	81	60	124	00	129	96
	56	81	76	125	86	129	118
	00	68	4E	126	00	104	78
	45	68	5F	127	69	104	95
	00	4F	3B	128	00	79	59
	35	4F	49	129	53	79	73
	00	FF	FF	130	00	255	255
	AA	FF	FF	131	170	255	255
	00	BD	BD	132	00	189	189
	7E	BD	BD	133	126	189	189
	00	81	81	134	00	129	129
	56	81	81	135	86	129	129
	00	68	68	136	00	104	104
	45	68	68	137	69	104	104
	00	4F	4F	138	00	79	79
	35	4F	4F	139	53	79	79
	00	BF	FF	140	00	191	255
	AA	EA	FF	141	170	234	255
	00	8D	BD	142	00	141	189
	7E	AD	BD	143	126	173	189
	00	60	81	144	00	96	129
	56	76	81	145	86	118	129
	00	4E	68	146	00	78	104
	45	5F	68	147	69	95	104
	00	3B	4F	148	00	59	79
	35	49	4F	149	53	73	79
	00	7F	FF	150	00	127	255
	AA	D4	FF	151	170	212	255
	00	5E	BD	152	00	94	189
	7E	9D	BD	153	126	157	189
	00	40	81	154	00	64	129
	56	6B	81	155	86	107	129
	00	34	68	156	00	52	104
	45	56	68	157	69	86	104
	00	27	4F	158	00	39	79
	35	42	4F	159	53	66	79
	00	3F	FF	160	00	63	255
	AA	BF	FF	161	170	191	255
	00	2E	BD	162	00	46	189
	7E	8D	BD	163	126	141	189
	00	1F	81	164	00	31	129
	56	60	81	165	86	96	129
	00	19	68	166	00	25	104
	45	4E	68	167	69	78	104
	00	13	4F	168	00	19	79
	35	3B	4F	169	53	59	79
	00	0	FF	170	00	0	255
	AA	AA	FF	171	170	170	255
	00	0	BD	172	00	0	189
	7E	7E	BD	173	126	126	189
	00	0	81	174	00	0	129
	56	56	81	175	86	86	129
	00	0	68	176	00	0	104
	45	45	68	177	69	69	104
	00	0	4F	178	00	0	79
	35	35	4F	179	53	53	79
	3F	00	FF	180	63	00	255
	BF	AA	FF	181	191	170	255
	2E	00	BD	182	46	00	189
	8D	7E	BD	183	141	126	189
	1F	00	81	184	31	00	129
	60	56	81	185	96	86	129
	19	00	68	186	25	00	104
	4E	45	68	187	78	69	104
	13	00	4F	188	19	00	79
	3B	35	4F	189	59	53	79
	7F	00	FF	190	127	00	255
	D4	AA	FF	191	212	170	255
	5E	00	BD	192	94	00	189
	9D	7E	BD	193	157	126	189
	40	00	81	194	64	00	129
	6B	56	81	195	107	86	129
	34	00	68	196	52	00	104
	56	45	68	197	86	69	104
	27	00	4F	198	39	00	79
	42	35	4F	199	66	53	79
	BF	00	FF	200	191	00	255
	EA	AA	FF	201	234	170	255
	8D	00	BD	202	141	00	189
	AD	7E	BD	203	173	126	189
	60	00	81	204	96	00	129
	76	56	81	205	118	86	129
	4E	00	68	206	78	00	104
	5F	45	68	207	95	69	104
	3B	00	4F	208	59	00	79
	49	35	4F	209	73	53	79
	FF	00	FF	210	255	00	255
	FF	AA	FF	211	255	170	255
	BD	00	BD	212	189	00	189
	BD	7E	BD	213	189	126	189
	81	00	81	214	129	00	129
	81	56	81	215	129	86	129
	68	00	68	216	104	00	104
	68	45	68	217	104	69	104
	4F	00	4F	218	79	00	79
	4F	35	4F	219	79	53	79
	FF	00	BF	220	255	00	191
	FF	AA	EA	221	255	170	234
	BD	00	8D	222	189	00	141
	BD	7E	AD	223	189	126	173
	81	00	60	224	129	00	96
	81	56	76	225	129	86	118
	68	00	4E	226	104	00	78
	68	45	5F	227	104	69	95
	4F	00	3B	228	79	00	59
	4F	35	49	229	79	53	73
	FF	00	7F	230	255	00	127
	FF	AA	D4	231	255	170	212
	BD	00	5E	232	189	00	94
	BD	7E	9D	233	189	126	157
	81	00	40	234	129	00	64
	81	56	6B	235	129	86	107
	68	00	34	236	104	00	52
	68	45	56	237	104	69	86
	4F	00	27	238	79	00	39
	4F	35	42	239	79	53	66
	FF	00	3F	240	255	00	63
	FF	AA	BF	241	255	170	191
	BD	00	2E	242	189	00	46
	BD	7E	8D	243	189	126	141
	81	00	1F	244	129	00	31
	81	56	60	245	129	86	96
	68	00	19	246	104	00	25
	68	45	4E	247	104	69	78
	4F	00	13	248	79	00	19
	4F	35	3B	249	79	53	59
	33	33	33	250	51	51	51
	50	50	50	251	80	80	80
	69	69	69	252	105	105	105
	82	82	82	253	130	130	130
	BE	BE	BE	254	190	190	190
	FF	FF	FF	255	255	255	255";

		private static List<AutocadColourCode> colourMap = new List<AutocadColourCode>();
		public ColourDictionary()
		{
			List<string> TableMapLines = ColourTableMap.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			foreach (string mapLine in TableMapLines)
			{
				if (string.IsNullOrEmpty(mapLine))
					continue;

				string[] arrFields = mapLine.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (arrFields != null && arrFields.Length == 7)
				{
					colourMap.Add(new AutocadColourCode
					{
						AutocadColourIndex = int.Parse(arrFields[3].Trim()),
						RGBCode = new RGBColourCode
						{
							Red = int.Parse(arrFields[4].Trim()),
							Blue = int.Parse(arrFields[5].Trim()),
							Green = int.Parse(arrFields[6].Trim())
						},
						HexCode = new HexColourCode
						{
							Red = arrFields[0].Trim(),
							Blue = arrFields[1].Trim(),
							Green = arrFields[2].Trim()
						}
					});
				}
			}
		}

		public string GetHexColourCodeToString(int autocadColourIndex)
		{
			try
			{
				return colourMap.Where(x => x.AutocadColourIndex.Value == autocadColourIndex).FirstOrDefault().HexCode.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : GetRGBColourCode(int autocadColourIndex) : " + ex.Message);
            }
            return "";
        }
		public string GetRGBColourCodeToString(int autocadColourIndex)
		{
			try
			{
				return colourMap.Where(x => x.AutocadColourIndex.Value == autocadColourIndex).FirstOrDefault().RGBCode.ToString();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : GetRGBColourCode(int autocadColourIndex) : " + ex.Message);
			}
			return "";
		}
		public HexColourCode GetHexColourCode(int autocadColourIndex)
		{
			try
			{
				return colourMap.Where(x => x.AutocadColourIndex.Value == autocadColourIndex).FirstOrDefault().HexCode;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : GetRGBColourCode(int autocadColourIndex) : " + ex.Message);
			}
			return null;
		}
		public RGBColourCode GetRGBColourCode(int autocadColourIndex)
		{
			try
			{
				return colourMap.Where(x => x.AutocadColourIndex.Value == autocadColourIndex).FirstOrDefault().RGBCode;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : GetRGBColourCode(int autocadColourIndex) : " + ex.Message);
			}
			return null;
		}
		public AutocadColourCode GetColourDetails(int autocadColourIndex)
		{
			try
			{
				return colourMap.Where(x => x.AutocadColourIndex.Value == autocadColourIndex).FirstOrDefault();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"GetColourDetails(int autocadColourIndex) : {autocadColourIndex} " + ex.Message);
			}
			return null;
		}
        public List<AutocadColourCode> GetAutocadeColourMap(int colourCode)
        {
            return colourMap;
        }
    }
}

