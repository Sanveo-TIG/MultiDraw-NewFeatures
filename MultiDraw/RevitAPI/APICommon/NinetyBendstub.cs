using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TIGUtility;

namespace MultiDraw
{
    public class NinetyBendstub
    {
        public static void GetSecondaryElements(Document doc, ref List<Element> pickedElements, out List<Element> secondaryElements,double stublength, string offSetVar,XYZ pickpoint)
        {
            int k = 0;
            double Baseelevation = 0;
            double correctspace = 0;
            secondaryElements = new List<Element>();
            Dictionary<double, List<Element>> groupedElements = new Dictionary<double, List<Element>>();
            Utility.GroupByElevation(pickedElements, offSetVar, ref groupedElements);
            pickedElements = new List<Element>();
            groupedElements = groupedElements.OrderByDescending(r => r.Key).ToDictionary(x => x.Key, x => x.Value);            
            foreach (KeyValuePair<double, List<Element>> valuePair in groupedElements)
            {
                List<Element> primaryElementsforOrder = valuePair.Value.OrderByDescending(r => ((r.Location as LocationCurve).Curve as Line).Origin.Y).ToList();
                XYZ firstStartPoint = ((primaryElementsforOrder[0].Location as LocationCurve).Curve as Line).GetEndPoint(0);
                XYZ firstEndPoint = ((primaryElementsforOrder[0].Location as LocationCurve).Curve as Line).GetEndPoint(1);
                double Elevation = primaryElementsforOrder[0].LookupParameter(offSetVar).AsDouble();
                if (k > 0)
                {
                    correctspace = Math.Sqrt(Math.Pow((Elevation - Baseelevation), 2));
                }
                for (int i = 0; i < primaryElementsforOrder.Count; i++)
                {
                    Conduit con = primaryElementsforOrder[i] as Conduit;
                    double conduitsize = con.LookupParameter("Outside Diameter").AsDouble();
                    Line l_Line = (primaryElementsforOrder[i].Location as LocationCurve).Curve as Line;
                    XYZ StartPoint = l_Line.GetEndPoint(0);
                    XYZ EndPoint = l_Line.GetEndPoint(1);

                    double SubdistanceOne = Math.Sqrt(Math.Pow((StartPoint.X - pickpoint.X), 2) + Math.Pow((StartPoint.Y - pickpoint.Y), 2));
                    double SubdistanceTwo = Math.Sqrt(Math.Pow((EndPoint.X - pickpoint.X), 2) + Math.Pow((EndPoint.Y - pickpoint.Y), 2));
                    XYZ ConduitStartpt = null;
                    XYZ ConduitEndpoint = null;

                    if (SubdistanceOne < SubdistanceTwo)
                    {
                        ConduitStartpt = StartPoint;
                        ConduitEndpoint = EndPoint;
                    }
                    else
                    {
                        ConduitStartpt = EndPoint;
                        ConduitEndpoint = StartPoint;
                    }

                    XYZ refStartPoint = ConduitStartpt;
                    Line Linefordirection = Line.CreateBound(ConduitEndpoint, ConduitStartpt);
                    Line RevereseLine = Line.CreateBound(ConduitStartpt, ConduitEndpoint);
                    XYZ LinefordirectionDir = Linefordirection.Direction;
                    XYZ ReverseDir = RevereseLine.Direction;
                    if (k > 0)
                    {
                        if (stublength > 0)
                        {
                            refStartPoint = refStartPoint + LinefordirectionDir.Multiply(correctspace + conduitsize);
                        }
                        else
                        {
                            refStartPoint = refStartPoint - LinefordirectionDir.Multiply(correctspace + conduitsize);
                        }
                       
                    }
                    XYZ refEndPoint = new XYZ(refStartPoint.X, refStartPoint.Y, refStartPoint.Z + stublength);
                    if (k > 0)
                    {
                        refEndPoint = new XYZ(refStartPoint.X, refStartPoint.Y, refEndPoint.Z + correctspace);
                    }

                    Conduit newCon = Utility.CreateConduit(doc, primaryElementsforOrder[i] as Conduit, refStartPoint, refEndPoint);
                    double elevation = primaryElementsforOrder[i].LookupParameter(offSetVar).AsDouble();
                    Element ele = doc.GetElement(newCon.Id);
                    SetElevation(ele, elevation, offSetVar);
                    pickedElements.Add(primaryElementsforOrder[i]);
                    secondaryElements.Add(ele);
                    Baseelevation = primaryElementsforOrder[i].LookupParameter(offSetVar).AsDouble();
                }
                ParentUserControl.Instance.Secondaryelst.Clear();
                ParentUserControl.Instance.Secondaryelst.AddRange(ParentUserControl.Instance.Primaryelst);
                ParentUserControl.Instance.Primaryelst.Clear();
                ParentUserControl.Instance.Primaryelst.AddRange(secondaryElements);
                k++;
            }
        }

        private static void SetElevation(Element Ele, double elevation, string offSetVar)
        {
            Parameter newElevation = Ele.LookupParameter(offSetVar);
            newElevation.Set(elevation);
        }
    }
}
