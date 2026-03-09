using EdmontonDrawingValidator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdmontonDrawingValidator {
    public sealed class SVGFileProcessor {
        public double minimumX = 1, minimumY = 1;
        public double maximumX = 1, maximumY = 1;

        public double minimumHeightPointY = 0, minimumWidthPointX = 0;
        public double maximumHeightPointY = 0, maximumWidthPointX = 0;

        public const int zoom = 5;
        public const string textSize = "0.2em";
        public const int textLen = 30;
        General objGeneral = new General();

        public double AdjustX(double x) {
            //---- old code
            //double diff = 0;
            //x = x * zoom;
            //diff = minimumWidthPointX * -1;
            //return objGeneral.FormatFigureInDecimalPoint(x + 1 + diff); // * (maximumX /  maximumWidthPointX);


            x = x * zoom;
            x = x - minimumWidthPointX;
            return objGeneral.FormatFigureInDecimalPoint(x);


        }

        public double AdjustY(double y) {
            //---- old code
            //double diff = 0;
            //y = y * zoom;
            //diff = minimumHeightPointY * -1;
            //return objGeneral.FormatFigureInDecimalPoint(maximumHeightPointY - ((y + 1 + diff) / (minimumY))); // minimumHeightPointY);

            y = y * zoom;
            y = y - minimumHeightPointY;
            y = y * -1;
            return objGeneral.FormatFigureInDecimalPoint(y);

        } 

        public void GetElementWidthAndHeight(dynamic points, ref double width, ref double height) {
            if (points != null && points.Count > 0) {

                double minX = points[0].X;
                double minY = points[0].Y;

                double maxX = points[0].X;
                double maxY = points[0].Y;

                foreach (dynamic point in points) {
                    if (point.X < minX) { minX = point.X; }
                    if (point.Y < minY) { minY = point.Y; }
                    if (point.X > maxX) { maxX = point.X; }
                    if (point.Y > maxY) { maxY = point.Y; }
                }
                width = AdjustX(maxX) - AdjustX(minX);
                height = Math.Abs(AdjustY(maxY) - AdjustY(minY));
            }
        }

        public string createSVG(string htmlTemplate, AllDrawingData allDrawingData) {

            string strokeWidth = "0.2";

            StringBuilder sb = new StringBuilder("");
            StringBuilder sbSpecial = new StringBuilder("");
            StringBuilder sbErrorLinks = new StringBuilder("");

            //https://stackoverflow.com/questions/15500894/background-color-of-text-in-svg
            //sb.Append($"<svg height=\"{((allDrawingData.Y.Maximum - allDrawingData.Y.Minimum) * zoom) + 1000}\" width=\"{((allDrawingData.X.Maximum - allDrawingData.X.Minimum) * zoom) + 1000}\" >");
            sb.Append(@"
                <defs>
                    <hatch id=""hatch"" hatchUnits=""userSpaceOnUse"" pitch=""5"" rotate=""135"" >
                        <hatchpath stroke=""#a080ff"" stroke-width=""2"" />
                    </hatch>

                    <filter x=""0"" y=""0"" width=""1"" height=""1"" id=""solid"">
                      <feFlood flood-color=""yellow"" result = ""bg"" />
                      <feMerge>
                        <feMergeNode in=""bg"" />
                        <feMergeNode in= ""SourceGraphic"" />
                      </feMerge>
                    </filter>

                    <filter x=""0"" y=""0"" width=""1"" height=""1"" id=""bg-text"" >
                      <feFlood flood-color = ""white"" />
                      <feComposite in=""SourceGraphic"" operator=""xor"" />
                    </filter>
                </defs> 
            ");

            //     sb.Append($"<svg viewBox=\"{((allDrawingData.X.Minimum) * zoom) } {((allDrawingData.Y.Minimum) * zoom)  } {((allDrawingData.X.Maximum - allDrawingData.X.Minimum)* zoom)} {((allDrawingData.Y.Maximum - allDrawingData.Y.Minimum) * zoom)  }\" >");

            minimumHeightPointY = objGeneral.FormatFigureInDecimalPoint(allDrawingData.Y.Minimum * zoom);
            minimumWidthPointX = objGeneral.FormatFigureInDecimalPoint(allDrawingData.X.Minimum * zoom);

            maximumHeightPointY = objGeneral.FormatFigureInDecimalPoint(allDrawingData.Y.Maximum * zoom);
            maximumWidthPointX = objGeneral.FormatFigureInDecimalPoint(allDrawingData.X.Maximum * zoom);

            maximumY = objGeneral.FormatFigureInDecimalPoint(allDrawingData.Y.Maximum - allDrawingData.Y.Minimum);
            maximumX = objGeneral.FormatFigureInDecimalPoint(allDrawingData.X.Maximum - allDrawingData.X.Minimum);


            //   allDrawingData.DrawingData = allDrawingData.DrawingData.Where(x => x.LayerName.ToLower() == "_building" || x.LayerName.ToLower() == "_floor").ToList();

            bool bFirstLink = true;
            ColourDictionary objColorDictionary = new ColourDictionary();

            int iErrorElement = 0;
            foreach (DrawingData objDrawing in allDrawingData.DrawingData) {
                var color = "#" + objColorDictionary.GetHexColourCodeToString(0);
                //if(objDrawing.Data.ColourCode.AutocadColourIndex != null && objDrawing.Data.ColourCode.AutocadColourIndex.Value != 0)
                //   color = "#" + objColorDictionary.GetHexColourCodeToString(objDrawing.Data.ColourCode.AutocadColourIndex.Value);

                if (objDrawing.TextInfo != null) {
                    if (objDrawing.LayerName.ToLower().Trim().Equals("_errorvalidatemessage")) {
                        iErrorElement++;

                        string temp = objDrawing.TextInfo.Text;
                        /*if (temp.Length > textLen)
                        {
                            temp = temp.Substring(0, textLen) + "...";
                        }*/
                        string sFirstLinkId = "";
                        if (objDrawing.TextInfo.Point == null) {
                            sbErrorLinks.AppendLine($"<div class=\"d-flex flex-row  align-items-baseline\">");
                            sbErrorLinks.AppendLine($"{temp}");
                            sbErrorLinks.AppendLine($"</div>");
                            continue;
                        }
                        else {
                            if (bFirstLink) {
                                bFirstLink = false;
                                sFirstLinkId = "id=\"myFirstLink\"";
                            }
                            double width = 0;
                            double height = 0;
                            GetElementWidthAndHeight(objDrawing.Data.Points, ref width, ref height);

                            sbErrorLinks.AppendLine($"<div class=\"d-flex flex-row  align-items-baseline\">");
                            sbErrorLinks.AppendLine($"<input class='chkError' type='Checkbox' elementId=\"{iErrorElement}\" >" +
                                $"&nbsp;<a class=\"d-flex text-decoration-none mt-1 mb-2 ps-1 \" " +
                                $"{sFirstLinkId} " +
                                $"width={width} " +
                                $"height={height} " +
                                //$"href=\"javascript:GoOnErrorPosition({AdjustX(objDrawing.TextInfo.Point.X)},{AdjustY(objDrawing.TextInfo.Point.Y)})\" " +
                                $"href=\"javascript:GoOnErrorPositionKNL({AdjustX(objDrawing.TextInfo.Point.X)},{AdjustY(objDrawing.TextInfo.Point.Y)},{width},{height},false)\" " +
                                $"alt=\"{objDrawing.TextInfo.Text}\">{temp}</a>");

                            sbErrorLinks.AppendLine($"</div>");
                        }
                        // sbSpecial.AppendLine($"<text id=\"ErrorShade{iErrorElement}\" filter=\"url(#bg-text)\" x=\"{AdjustX(objDrawing.TextInfo.Point.X)}\" y=\"{AdjustY(objDrawing.TextInfo.Point.Y)}\" fill=\"red\" font-size=\"{textSize}\" >{objDrawing.TextInfo.Text}</text>");
                        // sbSpecial.AppendLine($"<text id=\"ErrorText{iErrorElement}\" x=\"{AdjustX(objDrawing.TextInfo.Point.X)}\" y=\"{AdjustY(objDrawing.TextInfo.Point.Y)}\" fill=\"red\" font-size=\"{textSize}\" >{objDrawing.TextInfo.Text}</text>");
                    }
                    else {
                        sb.AppendLine($"<text x=\"{AdjustX(objDrawing.TextInfo.Point.X)}\" y=\"{AdjustY(objDrawing.TextInfo.Point.Y)}\" font-size=\"{textSize}\" alt=\"{objDrawing.TextInfo.Text}\"  fill=\"{color}\" >{objDrawing.TextInfo.Text}</text>");
                        // sb.AppendLine($"<!-- <text x=\"{(objDrawing.TextInfo.Point.X)}\" y=\"{(objDrawing.TextInfo.Point.Y)}\" >{objDrawing.TextInfo.Text}</text> -->");
                    }
                }

                if (objDrawing.Data.IsCircle) {
                    sb.AppendLine($"<circle cx=\"{AdjustX(objDrawing.Data.CenterPoint.X)}\" cy=\"{AdjustY(objDrawing.Data.CenterPoint.Y)}\" r=\"{objDrawing.Data.Radius}\" stroke=\"{color}\" stroke-width=\"{strokeWidth}\"  fill=\"none\" />");
                }

                if (!objDrawing.Data.IsCircle && !objDrawing.Data.HasBulge) {
                    if (objDrawing.Data.Points != null) {
                        //if (objDrawing.Data.Points.Count == 2)
                        //{
                        //    sb.AppendLine($"<line x1=\"{AdjustX(objDrawing.Data.Points[0].X)}\" y1=\"{AdjustY(objDrawing.Data.Points[0].Y)}\" x2=\"{AdjustX(objDrawing.Data.Points[1].X)}\" y2=\"{AdjustY(objDrawing.Data.Points[1].Y)}\" stroke=\"{color}\" stroke-width=\"{strokeWidth}\" fill=\"none\"  />");
                        //}
                        //else
                        if (objDrawing.Data.Points.Count > 1) {

                            sb.Append($"<polyline  points=\"");
                            for (int i = 0; i < objDrawing.Data.Points.Count; i++) {
                                sb.Append($"{AdjustX(objDrawing.Data.Points[i].X)},{AdjustY(objDrawing.Data.Points[i].Y)} ");
                            }

                            if (objDrawing.LayerName.ToLower().Trim().Equals("_errorvalidatemessage")) {
                                sb.Append($"\" id=\"Error{iErrorElement}\" stroke=\"red\" stroke-width=\"{strokeWidth}\" ");
                                sb.AppendLine(@" fill=""url(#hatch)"" /> ");
                            }
                            else {
                                sb.Append($"\" stroke=\"{color}\" stroke-width=\"{strokeWidth}\" ");
                                sb.AppendLine(" fill=\"none\" /> ");
                            }
                        }
                    }
                }
            }
            sb.AppendLine(sbSpecial.ToString());
            //sb.AppendLine("</svg>");

            //sb.AppendLine(sbErrorLinks.ToString());


            //--------Khushbu reference
            double WholeDrawing_MinX = AdjustX(allDrawingData.X.Minimum);
            double WholeDrawing_MinY = AdjustY(allDrawingData.Y.Maximum);
            double WholeDrawing_MaxX = AdjustX(allDrawingData.X.Maximum);
            double WholeDrawing_MaxY = AdjustY(allDrawingData.Y.Minimum);
            double WholeDrawing_Width = Math.Round(Math.Abs(WholeDrawing_MaxX - WholeDrawing_MinX), 2);
            double WholeDrawing_Height = Math.Round(Math.Abs(WholeDrawing_MaxY - WholeDrawing_MinY), 2);

            htmlTemplate = htmlTemplate.Replace("[minx]", ((int)WholeDrawing_MinX).ToString());
            htmlTemplate = htmlTemplate.Replace("[maxx]", ((int)WholeDrawing_MaxX).ToString());

            htmlTemplate = htmlTemplate.Replace("[miny]", ((int)WholeDrawing_MinY).ToString());
            htmlTemplate = htmlTemplate.Replace("[maxy]", ((int)WholeDrawing_MaxY).ToString());

            htmlTemplate = htmlTemplate.Replace("[centerx]", ((Int64)AdjustX((WholeDrawing_MinX + WholeDrawing_MaxX) / 2)).ToString());
            htmlTemplate = htmlTemplate.Replace("[centery]", ((Int64)AdjustY((WholeDrawing_MinY + WholeDrawing_MaxY) / 2)).ToString());


            htmlTemplate = htmlTemplate.Replace("{width}", (WholeDrawing_Width + 10).ToString());
            htmlTemplate = htmlTemplate.Replace("{height}", (WholeDrawing_Height + 10).ToString());
            htmlTemplate = htmlTemplate.Replace("[dwidth]", WholeDrawing_Width.ToString());
            htmlTemplate = htmlTemplate.Replace("[dheight]", WholeDrawing_Height.ToString());
            htmlTemplate = htmlTemplate.Replace("[darea]", Math.Round((WholeDrawing_Width * WholeDrawing_Height), 2).ToString());

            double Area = (Int64)Math.Abs((WholeDrawing_Width * WholeDrawing_Height));
            int AreaDigits = Area.ToString().Length;
            Int64 MaxZoomLevel = 10 * AreaDigits;

            htmlTemplate = htmlTemplate.Replace("[minzoom]", "0");
            htmlTemplate = htmlTemplate.Replace("[maxzoom]", MaxZoomLevel.ToString());

            htmlTemplate = htmlTemplate.Replace("[homex]", (WholeDrawing_MinX + 100).ToString());
            htmlTemplate = htmlTemplate.Replace("[homey]", (WholeDrawing_MinY + 100).ToString());
            htmlTemplate = htmlTemplate.Replace("[homewidth]", WholeDrawing_Width.ToString());
            htmlTemplate = htmlTemplate.Replace("[homeheight]", WholeDrawing_Height.ToString());


            htmlTemplate = htmlTemplate.Replace("{SVG_DATA}", sb.ToString());
            htmlTemplate = htmlTemplate.Replace("{LINK_DATA}", sbErrorLinks.ToString());

            return htmlTemplate;
        }
    }
}