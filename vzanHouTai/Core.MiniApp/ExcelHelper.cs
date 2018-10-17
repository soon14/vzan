using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using System.Collections;
using System.Text;
using System.Reflection;

using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Core.MiniApp
{
    public static class ExcelHelper<T>
    {
        #region 导出Excel
        /// <summary>
        /// 导出Excel(非模板),导出的单元格格式统一设置为文本格式
        /// </summary>
        /// <param name="dtSource">数据来源</param>
        /// <param name="strFileName">输出文件名称</param>
        public static void Out2Excel(DataTable dtSource, string fileName)
        {
            #region tjm20130607修改当行超过65536时把数据分到其他的sheet
            //HSSFWorkbook workbook = new HSSFWorkbook();
            //HSSFSheet sheet = workbook.CreateSheet() as HSSFSheet;


            ////填充表头   
            //HSSFRow dataRow = sheet.CreateRow(0) as HSSFRow;
            //foreach (DataColumn column in dtSource.Columns)
            //{
            //    HSSFCell cell = dataRow.CreateCell(column.Ordinal) as HSSFCell;
            //    cell.SetCellValue(column.ColumnName);
            //}


            ////填充内容   
            //for (int i = 0; i < dtSource.Rows.Count; i++)
            //{
            //    dataRow = sheet.CreateRow(i + 1) as HSSFRow;
            //    for (int j = 0; j < dtSource.Columns.Count; j++)
            //    {
            //        HSSFCell cell = dataRow.CreateCell(j) as HSSFCell;
            //        cell.CellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");
            //        cell.SetCellValue(dtSource.Rows[i][j].ToString());
            //    }
            //}

            ////保存   
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    workbook.Write(ms);

            //    Out2Client(ms, fileName);
            //}
            //sheet.Dispose();
            //workbook.Dispose();
            #endregion

            HSSFWorkbook workbook = new HSSFWorkbook();
            HSSFSheet sheet = workbook.CreateSheet() as HSSFSheet;

            HSSFRow dataRow = null;
            HSSFCell cell = null;
            int sheetMaxRowIndex = 65535;
            int currentRowIndex = 0;
            //填充内容   
            for (int i = 0; i < dtSource.Rows.Count; i++)
            {
                if (currentRowIndex == 0)
                {
                    //填充表头   
                    dataRow = sheet.CreateRow(currentRowIndex) as HSSFRow;
                    foreach (DataColumn column in dtSource.Columns)
                    {
                        cell = dataRow.CreateCell(column.Ordinal) as HSSFCell;
                        cell.SetCellValue(column.ColumnName);
                    }
                    currentRowIndex++;
                }


                dataRow = sheet.CreateRow(currentRowIndex) as HSSFRow;
                for (int j = 0; j < dtSource.Columns.Count; j++)
                {
                    cell = dataRow.CreateCell(j) as HSSFCell;
                    //cell.CellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");

                    cell.SetCellValue(dtSource.Rows[i][j].ToString());
                }
                currentRowIndex++;
                if (currentRowIndex > sheetMaxRowIndex)
                {
                    sheet = workbook.CreateSheet() as HSSFSheet;
                    currentRowIndex = 0;
                }
            }

            //保存   
            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                //sheet.Dispose();
                //workbook.Dispose();
                cell = null;
                dataRow = null;
                sheet = null;
                workbook = null;
                dtSource = null;
                Out2Client(ms, fileName);
            }

        }


        public static void Out2ExcelDataReader(System.Data.SqlClient.SqlDataReader dr, string fileName)
        {
            HSSFWorkbook workbook = new HSSFWorkbook();
            HSSFSheet sheet = workbook.CreateSheet() as HSSFSheet;

            HSSFRow dataRow = null;
            HSSFCell cell = null;
            int sheetMaxRowIndex = 65535;
            int currentRowIndex = 0;


            List<string> drColumnNameList = new List<string>();
            for (int i = 0; i < dr.FieldCount; i++)
            {
                drColumnNameList.Add(dr.GetName(i));
            }
            while (dr.Read())
            {

                if (currentRowIndex == 0)
                {
                    //填充表头   
                    dataRow = sheet.CreateRow(currentRowIndex) as HSSFRow;
                    for (int i = 0; i < drColumnNameList.Count; i++)
                    {
                        cell = dataRow.CreateCell(i) as HSSFCell;
                        cell.SetCellValue(drColumnNameList[i]);
                    }
                    currentRowIndex++;
                }
                dataRow = sheet.CreateRow(currentRowIndex) as HSSFRow;
                for (int i = 0; i < drColumnNameList.Count; i++)
                {
                    cell = dataRow.CreateCell(i) as HSSFCell;
                    cell.CellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");
                    cell.SetCellValue(dr[i].ToString());
                }
                currentRowIndex++;
                if (currentRowIndex > sheetMaxRowIndex)
                {
                    sheet = workbook.CreateSheet() as HSSFSheet;
                    currentRowIndex = 0;
                }
            }
            dr.Close();
            dr.Dispose();

            //保存   
            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                //sheet.Dispose();
                //workbook.Dispose();
                cell = null;
                dataRow = null;
                sheet = null;
                workbook = null;
                Out2Client(ms, fileName);
            }

        }


        public static void OutHtml2ExcelDataReader(System.Data.SqlClient.SqlDataReader dr, string fileName)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("<table border='1'>");

            int sheetMaxRowIndex = 65535;
            int currentRowIndex = 0;

            List<string> drColumnNameList = new List<string>();
            for (int i = 0; i < dr.FieldCount; i++)
            {
                drColumnNameList.Add(dr.GetName(i));
            }
            while (dr.Read())
            {

                if (currentRowIndex == 0)
                {
                    //填充表头   
                    sb.Append("<tr>");
                    for (int i = 0; i < drColumnNameList.Count; i++)
                    {
                        sb.Append("<td>");
                        sb.Append(drColumnNameList[i]);
                        sb.Append("</td>");
                    }
                    sb.Append("</tr>");
                    currentRowIndex++;
                }



                sb.Append("<tr>");
                for (int i = 0; i < drColumnNameList.Count; i++)
                {
                    sb.Append("<td>");
                    if (dr[i].ToString() != "")
                    {
                        sb.Append(dr[i].ToString());
                    }
                    else
                    {
                        sb.Append("&nbsp;");
                    }
                    sb.Append("</td>");

                }
                sb.Append("</tr>");

                currentRowIndex++;
                if (currentRowIndex > sheetMaxRowIndex)
                {
                    break;
                }
            }
            sb.Append("</table>");
            dr.Close();
            dr.Dispose();

            //保存   
            Out2ClientByString(sb.ToString(), fileName);
        }


        public static void OutCsvDataReader(System.Data.SqlClient.SqlDataReader dr, string fileName)
        {
            StringBuilder sb = new StringBuilder();

            //int sheetMaxRowIndex = 65535;
            int currentRowIndex = 0;

            List<string> drColumnNameList = new List<string>();
            for (int i = 0; i < dr.FieldCount; i++)
            {
                drColumnNameList.Add(dr.GetName(i));
            }
            while (dr.Read())
            {

                if (currentRowIndex == 0)
                {
                    //填充表头   
                    for (int i = 0; i < drColumnNameList.Count; i++)
                    {
                        sb.Append("\"");
                        sb.Append(drColumnNameList[i]);
                        sb.Append("\",");
                    }
                    sb.Append("\r\n");
                    currentRowIndex++;
                }

                for (int i = 0; i < drColumnNameList.Count; i++)
                {
                    sb.Append("\"");
                    sb.Append(dr[i].ToString());
                    sb.Append("\",");
                }
                sb.Append("\r\n");

                currentRowIndex++;
                //if (currentRowIndex > sheetMaxRowIndex)
                //{
                //    break;
                //}
            }
            dr.Close();
            dr.Dispose();

            //保存   
            Out2ClientCsv(sb.ToString(), fileName);
        }



        /// <summary>
        /// 输出到客户端
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="fileName">文件名</param>
        private static void Out2Client(MemoryStream ms, string fileName)
        {
            byte[] data = ms.ToArray();
            ms.Close();
            ms.Dispose();
            ms = null;
            GC.Collect();
            if (!fileName.Contains(".xls") || !fileName.Contains(".xlsx"))
            {
                fileName += ".xls";
            }

            #region 客户端保存
            HttpResponse response = HttpContext.Current.Response;
            HttpRequest request = HttpContext.Current.Request;
            response.Clear();
            response.Charset = "UTF-8";
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.ContentType = "application/vnd-excel";//"application/vnd.ms-excel";
            if (request.UserAgent.ToLower().IndexOf("msie") > -1)
            {
                fileName = HttpUtility.UrlPathEncode(fileName);
            }
            if (request.UserAgent.ToLower().IndexOf("firefox") > -1)
            {
                response.AddHeader("Content-Disposition", "attachment;filename=\"" + fileName + "\"");
            }
            else
            {
                response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
            }
            response.AddHeader("Content-Length", data.Length.ToString());
            response.BinaryWrite(data);
            response.End();
            #endregion
        }


        /// <summary>
        /// 输出到客户端
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="fileName">文件名</param>
        private static void Out2ClientByString(string data, string fileName)
        {
            if (!fileName.Contains(".xls") || !fileName.Contains(".xlsx"))
            {
                fileName += ".xls";
            }

            #region 客户端保存
            HttpResponse response = HttpContext.Current.Response;
            HttpRequest request = HttpContext.Current.Request;
            response.Clear();
            response.Charset = "UTF-8";
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.ContentType = "application/vnd-excel";//"application/vnd.ms-excel";
            if (request.UserAgent.ToLower().IndexOf("msie") > -1)
            {
                fileName = HttpUtility.UrlPathEncode(fileName);
            }
            if (request.UserAgent.ToLower().IndexOf("firefox") > -1)
            {
                response.AddHeader("Content-Disposition", "attachment;filename=\"" + fileName + "\"");
            }
            else
            {
                response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
            }
            //  response.BinaryWrite(data);
            response.Write(data);
            response.End();
            #endregion
        }

        /// <summary>
        /// 输出到客户端
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="fileName">文件名</param>
        private static void Out2ClientCsv(string data, string fileName)
        {
            if (!fileName.Contains(".csv"))
            {
                fileName += ".csv";
            }

            #region 客户端保存
            HttpResponse response = HttpContext.Current.Response;
            HttpRequest request = HttpContext.Current.Request;
            response.Clear();
            //  response.Charset = "UTF-8";
            response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");
            response.ContentType = "application/vnd-excel";//"application/vnd.ms-excel";
            if (request.UserAgent.ToLower().IndexOf("msie") > -1)
            {
                fileName = HttpUtility.UrlPathEncode(fileName);
            }
            if (request.UserAgent.ToLower().IndexOf("firefox") > -1)
            {
                response.AddHeader("Content-Disposition", "attachment;filename=\"" + fileName + "\"");
            }
            else
            {
                response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
            }
            response.Write(data);
            response.End();
            #endregion
        }

        /// <summary>
        /// 利用模板，DataTable导出到Excel（单个类别）
        /// </summary>
        /// <param name="dtSource">DataTable</param>
        /// <param name="strFileName">生成的文件路径、名称</param>
        /// <param name="strTemplateFileName">模板的文件路径、名称</param>
        /// <param name="sheetName">文件标识</param>
        /// <param name="titleName">表头名称</param>
        public static void Out2ExcelByTemplate(DataTable dtSource, string strFileName, string strTemplateFileName, string sheetName, string titleName)
        {
            // 利用模板，DataTable导出到Excel（单个类别）
            using (MemoryStream ms = GetMemoryStreamForOut2ExcelByTemplate(dtSource, strTemplateFileName, sheetName, titleName))
            {
                Out2Client(ms, strFileName);
            }
        }


        /// <summary>
        /// 利用模板，DataTable导出到Excel（单个类别）
        /// </summary>
        /// <param name="dtSource">DataTable</param>
        /// <param name="strTemplateFileName">模板的文件路径、名称</param>
        /// <param name="sheetName">文件标识--sheet名</param>
        /// <param name="titleName">表头名称</param>
        /// <returns></returns>
        private static MemoryStream GetMemoryStreamForOut2ExcelByTemplate(DataTable dtSource, string strTemplateFileName, string sheetName, string titleName)
        {
            int rowIndex = 2;       // 起始行
            //int dtRowIndex = dtSource.Rows.Count;       // DataTable的数据行数

            FileStream file = new FileStream(strTemplateFileName, FileMode.Open, FileAccess.Read);//读入excel模板
            HSSFWorkbook workbook = new HSSFWorkbook(file);

            if (string.IsNullOrEmpty(sheetName))
            {
                sheetName = "Sheet1";
            }

            HSSFSheet sheet = workbook.GetSheet(sheetName) as HSSFSheet;

            #region 右击文件 属性信息
            {
                //DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
                //dsi.Company = "农发集团";
                //workbook.DocumentSummaryInformation = dsi;

                //SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
                //si.Author = "农发集团"; //填加xls文件作者信息
                //si.ApplicationName = "瞬时达"; //填加xls文件创建程序信息
                //si.LastAuthor = "瞬时达"; //填加xls文件最后保存者信息
                //si.Comments = "瞬时达"; //填加xls文件作者信息
                //si.Title = "农发集团报表"; //填加xls文件标题信息
                //si.Subject = "农发集团报表";//填加文件主题信息
                //si.CreateDateTime = DateTime.Now;
                //workbook.SummaryInformation = si;
            }
            #endregion

            #region 表头
            HSSFRow headerRow = sheet.GetRow(0) as HSSFRow;
            HSSFCell headerCell = headerRow.GetCell(0) as HSSFCell;
            headerCell.SetCellValue(titleName);
            #endregion

            foreach (DataRow row in dtSource.Rows)
            {
                #region 填充内容
                HSSFRow dataRow = sheet.GetRow(rowIndex) as HSSFRow;

                int columnIndex = 1;        // 开始列（0为标题列，从1开始）
                foreach (DataColumn column in dtSource.Columns)
                {
                    // 列序号赋值
                    if (columnIndex >= dtSource.Columns.Count)
                        break;

                    HSSFCell newCell = dataRow.GetCell(columnIndex) as HSSFCell;
                    if (newCell == null)
                        newCell = dataRow.CreateCell(columnIndex) as HSSFCell;

                    string drValue = row[column].ToString();

                    switch (column.DataType.ToString())
                    {
                        case "System.String"://字符串类型
                            newCell.CellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");
                            newCell.SetCellValue(drValue);
                            break;
                        case "System.DateTime"://日期类型
                            DateTime dateV;
                            DateTime.TryParse(drValue, out dateV);
                            newCell.SetCellValue(dateV);
                            break;
                        case "System.Boolean"://布尔型
                            bool boolV = false;
                            bool.TryParse(drValue, out boolV);
                            newCell.SetCellValue(boolV);
                            break;
                        case "System.Int16"://整型
                        case "System.Int32":
                        case "System.Int64":
                        case "System.Byte":
                            int intV = 0;
                            int.TryParse(drValue, out intV);
                            newCell.SetCellValue(intV);
                            break;
                        case "System.Decimal"://浮点型
                        case "System.Double":
                            double doubV = 0;
                            double.TryParse(drValue, out doubV);
                            newCell.SetCellValue(doubV);
                            break;
                        case "System.DBNull"://空值处理
                            newCell.SetCellValue("");
                            break;
                        default:
                            newCell.SetCellValue("");
                            break;
                    }
                    columnIndex++;
                }
                #endregion

                rowIndex++;
            }

            // 格式化当前sheet，用于数据total计算
            sheet.ForceFormulaRecalculation = true;

            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
                sheet = null;
                workbook = null;


                //sheet.Dispose();
                //workbook.Dispose();//一般只用写这一个就OK了，他会遍历并释放所有资源，但当前版本有问题所以只释放sheet
                return ms;
            }
        }



        /// <summary>
        /// 泛型集合类导出成excel
        /// </summary>
        /// <param name="list">泛型集合类</param>
        /// <param name="fileName">生成的excel文件名</param>
        /// <param name="propertyName">excel的字段列表</param>
        public static void ListToExcel(IList<T> list, string fileName, string[] headerName, params string[] propertyName)
        {
            //HttpContext.Current.Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            //HttpContext.Current.Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", fileName));
            //HttpContext.Current.Response.Clear();
            //HttpContext.Current.Response.BinaryWrite(ListToExcel<T>(list, propertyName).GetBuffer());
            //HttpContext.Current.Response.End();
            MemoryStream ms = ListToExcel<T>(list, headerName, propertyName);
            Out2Client(ms, fileName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="headerName">列名必须与propertyName一一对应</param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static MemoryStream ListToExcel<M>(IList<M> list, string[] headerName, params string[] propertyName)
        {
            //创建流对象
            using (MemoryStream ms = new MemoryStream())
            {
                //将参数写入到一个临时集合中
                List<string> propertyNameList = new List<string>();
                if (propertyName != null)
                    propertyNameList.AddRange(propertyName);
                // 床NOPI的相关对象
                //////Workbook workbook = new HSSFWorkbook();
                //////Sheet sheet = workbook.CreateSheet();
                //////Row headerRow = sheet.CreateRow(0);

                //////if (list.Count > 0)
                //////{
                //////    //通过反射得到对象的属性集合
                //////    PropertyInfo[] propertys = list[0].GetType().GetProperties();
                //////    //遍历属性集合生成excel的表头标题
                //////    for (int i = 0; i < propertys.Count(); i++)
                //////    {
                //////        //判断此属性是否是用户定义属性
                //////        if (propertyNameList.Count == 0)
                //////        {
                //////            headerRow.CreateCell(i).SetCellValue(propertys[i].Name);
                //////        }
                //////        else
                //////        {
                //////            if (propertyNameList.Contains(propertys[i].Name))
                //////                headerRow.CreateCell(i).SetCellValue(headerName[i]);
                //////        }
                //////    }


                //////    int rowIndex = 1;
                //////    //遍历集合生成excel的行集数据
                //////    for (int i = 0; i < list.Count; i++)
                //////    {
                //////        Row dataRow = sheet.CreateRow(rowIndex);
                //////        for (int j = 0; j < propertys.Count(); j++)
                //////        {
                //////            if (propertyNameList.Count == 0)
                //////            {
                //////                object obj = propertys[j].GetValue(list[i], null);
                //////                dataRow.CreateCell(j).SetCellValue(obj.ToString());
                //////            }
                //////            else
                //////            {
                //////                if (propertyNameList.Contains(propertys[j].Name))
                //////                {
                //////                    object obj = propertys[j].GetValue(list[i], null);
                //////                    dataRow.CreateCell(j).SetCellValue(obj.ToString());
                //////                }
                //////            }
                //////        }
                //////        rowIndex++;
                //////    }
                //////}
                //////workbook.Write(ms);
                //////ms.Flush();
                //////ms.Position = 0;
                return ms;
            }
        }
        /// <summary>
        /// 泛型集合类导出成全部为文本类型的excel
        /// </summary>
        /// <param name="list">泛型集合类</param>
        /// <param name="fileName">生成的excel文件名</param>
        /// <param name="propertyName">excel的字段列表</param>
        public static void ListToExcelAllString(IList<T> list, string fileName, string[] headerName, params string[] propertyName)
        {
            //HttpContext.Current.Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            //HttpContext.Current.Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", fileName));
            //HttpContext.Current.Response.Clear();
            //HttpContext.Current.Response.BinaryWrite(ListToExcel<T>(list, propertyName).GetBuffer());
            //HttpContext.Current.Response.End();
            MemoryStream ms = ListToExcelAllstring<T>(list, headerName, propertyName);
            Out2Client(ms, fileName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="headerName">列名必须与propertyName一一对应</param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static MemoryStream ListToExcelAllstring<M>(IList<M> list, string[] headerName, params string[] propertyName)
        {
            //创建流对象
            using (MemoryStream ms = new MemoryStream())
            {
                //将参数写入到一个临时集合中
                List<string> propertyNameList = new List<string>();
                if (propertyName != null)
                    propertyNameList.AddRange(propertyName);
                //床NOPI的相关对象
                HSSFWorkbook demoWorkBook = new HSSFWorkbook();
                HSSFSheet demoSheet = demoWorkBook.CreateSheet() as HSSFSheet;

                if (list.Count > 0)
                {
                    //通过反射得到对象的属性集合
                    PropertyInfo[] propertys = list[0].GetType().GetProperties();
                    HSSFRow dataRow = demoSheet.CreateRow(0) as HSSFRow;
                    //遍历属性集合生成excel的表头标题
                    for (int i = 0; i < propertys.Count(); i++)
                    {
                        HSSFCell cell = dataRow.CreateCell(i, CellType.String) as HSSFCell;
                        //判断此属性是否是用户定义属性
                        if (propertyNameList.Count == 0)
                        {
                            cell.SetCellValue(propertys[i].Name);
                        }
                        else
                        {
                            if (propertyNameList.Contains(propertys[i].Name))
                                cell.SetCellValue(headerName[i]);
                        }

                    }


                    int rowIndex = 1;
                    //遍历集合生成excel的行集数据
                    for (int i = 0; i < list.Count; i++)
                    {
                        dataRow = demoSheet.CreateRow(rowIndex) as HSSFRow;
                        for (int j = 0; j < propertys.Count(); j++)
                        {
                            HSSFCell cell = dataRow.CreateCell(j, CellType.String) as HSSFCell;
                            if (propertyNameList.Count == 0)
                            {
                                object obj = propertys[j].GetValue(list[i], null);
                                cell.SetCellValue(obj.ToString());
                                cell.CellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");
                            }
                            else
                            {
                                if (propertyNameList.Contains(propertys[j].Name))
                                {
                                    object obj = propertys[j].GetValue(list[i], null);
                                    cell.SetCellValue(obj.ToString());
                                    cell.CellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");

                                }
                            }
                        }
                        rowIndex++;
                    }
                }
                demoWorkBook.Write(ms);
                ms.Flush();
                ms.Position = 0;
                return ms;
            }
        }

        #endregion

        #region 导入Excel
        /// <summary>
        /// 导入Excel
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DataTable Upload2DataTable(string path)
        {
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                HSSFWorkbook hssfworkbook = new HSSFWorkbook(file);
                HSSFSheet sheet = hssfworkbook.GetSheetAt(0) as HSSFSheet;

                IEnumerator rows = sheet.GetRowEnumerator();

                DataTable dt = new DataTable();
                //创建列名，以第一行为名
                if (rows.MoveNext())
                {
                    HSSFRow row = rows.Current as HSSFRow;
                    for (int i = 0; i < row.LastCellNum; i++)
                    {
                        HSSFCell cell = row.GetCell(i) as HSSFCell;
                        if (cell == null)
                            dt.Columns.Add("无效");
                        else
                            dt.Columns.Add(cell.ToString());
                    }
                }
                //数据从第二行开始
                while (rows.MoveNext())
                {
                    HSSFRow row = rows.Current as HSSFRow;
                    DataRow dr = dt.NewRow();

                    for (int i = 0; i < row.LastCellNum; i++)
                    {
                        GetCellValue(row, dr, i);
                    }

                    dt.Rows.Add(dr);
                }
                return dt;
            }
        }

        /// <summary>
        /// 文件为xls  2003
        /// </summary>
        /// <param name="row"></param>
        /// <param name="dr"></param>
        /// <param name="i"></param>
        private static void GetCellValue(HSSFRow row, DataRow dr, int i)
        {
            
            HSSFCell cell = row.GetCell(i) as HSSFCell;
            if (cell != null)
            {
                switch (cell.CellType)
                {
                    case CellType.Blank:
                        dr[i] = null;
                        break;
                    case CellType.Boolean:
                        dr[i] = cell.BooleanCellValue;
                        break;
                    case CellType.Numeric:
                        ////This is a trick to get the correct value of the cell. NumericCellValue will return a numeric value no matter the cell value is a date or a number.
                        if (DateTime.Compare(cell.DateCellValue, DateTime.Parse("1900-01-01")) > 0)
                            dr[i] = cell.DateCellValue;
                        else
                            dr[i] = cell.ToString();
                        //if (HSSFDateUtil.IsCellDateFormatted(cell))
                        //{
                        //    dr[i] = cell.DateCellValue;
                        //}
                        //if (cell.CellType == NPOI.SS.UserModel.CellType.NUMERIC)
                        //{
                        //    dr[i] = cell.NumericCellValue;
                        //}

                        break;
                    case CellType.String:
                        dr[i] = cell.StringCellValue;
                        break;
                    case CellType.Error:
                        dr[i] = cell.ErrorCellValue;
                        break;
                    case CellType.Formula:
                    default:
                        dr[i] = cell.NumericCellValue;
                        break;
                }
            }
        }

        /// <summary>
        /// 文件为xlsx 2007
        /// </summary>
        /// <param name="row"></param>
        /// <param name="dr"></param>
        /// <param name="i"></param>
        private static void GetCellValue(XSSFRow row, DataRow dr, int i)
        {
            XSSFCell cell = row.GetCell(i) as XSSFCell;
            if (cell != null)
            {
                switch (cell.CellType)
                {
                    case CellType.Blank:
                        dr[i] = null;
                        break;
                    case CellType.Boolean:
                        dr[i] = cell.BooleanCellValue;
                        break;
                    case CellType.Numeric:
                        ////This is a trick to get the correct value of the cell. NumericCellValue will return a numeric value no matter the cell value is a date or a number.
                        if (DateTime.Compare(cell.DateCellValue, DateTime.Parse("1900-01-01")) > 0)
                            dr[i] = cell.DateCellValue;
                        else
                            dr[i] = cell.ToString();
                        //if (HSSFDateUtil.IsCellDateFormatted(cell))
                        //{
                        //    dr[i] = cell.DateCellValue;
                        //}
                        //if (cell.CellType == NPOI.SS.UserModel.CellType.NUMERIC)
                        //{
                        //    dr[i] = cell.NumericCellValue;
                        //}

                        break;
                    case CellType.String :
                        dr[i] = cell.StringCellValue;
                        break;
                    case CellType.Error:
                        dr[i] = cell.ErrorCellValue;
                        break;
                    case CellType.Formula:
                    default:
                        dr[i] = cell.NumericCellValue;
                        break;
                }
            }
        }
        #endregion

        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <returns>返回的DataTable</returns>
        public static DataTable Excel2003ToDataTable(Stream stream, bool isFirstRowColumn = true, string sheetName = null)
        {
            ISheet sheet = null;
            DataTable data = new DataTable();
            int startRow = 0;
            try
            {
            
                HSSFWorkbook workbook = new HSSFWorkbook(stream);

                if (sheetName != null)
                {
                    sheet = workbook.GetSheet(sheetName);
                    if (sheet == null) //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                }
                else
                {
                    sheet = workbook.GetSheetAt(0);
                }
                if (sheet != null)
                {
                    IRow firstRow = sheet.GetRow(0);
                    int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }
                    else
                    {
                        startRow = sheet.FirstRowNum;
                    }

                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        DataRow dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            GetCellValue(row as HSSFRow, dataRow, j);
                        }
                        data.Rows.Add(dataRow);
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(ExcelHelper<T>), ex);
                return null;
            }
        }


        public static DataTable Excel2007ToDataTable(Stream stream, bool isFirstRowColumn = true, string sheetName = null)
        {
            ISheet sheet = null;
            DataTable data = new DataTable();
            int startRow = 0;
            try
            {

                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                if (sheetName != null)
                {
                    sheet = workbook.GetSheet(sheetName);
                    if (sheet == null) //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                }
                else
                {
                    sheet = workbook.GetSheetAt(0);
                }
                if (sheet != null)
                {
                    IRow firstRow = sheet.GetRow(0);
                    int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }
                    else
                    {
                        startRow = sheet.FirstRowNum;
                    }

                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        DataRow dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            GetCellValue(row as XSSFRow, dataRow, j);
                            //GetCellValue(row as HSSFRow, dataRow, j);
                        }
                        data.Rows.Add(dataRow);
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(ExcelHelper<T>), ex);
                return null;
            }
        }


        /// <summary> 
        /// 将一组对象导出成EXCEL 
        /// </summary> 
        /// <typeparam name="T">要导出对象的类型</typeparam> 
        /// <param name="objList">一组对象</param> 
        /// <param name="FileName">导出后的文件名</param> 
        /// <param name="columnInfo">列名字段名及对应列表名称</param> 
        public static void ExExcel(List<T> objList, string FileName, Dictionary<string, string> columnInfo)
        {
            if (columnInfo.Count == 0) { return; }
            if (objList.Count == 0) { return; }
            //生成EXCEL的HTML 
            string excelStr = "";
            Type myType = objList[0].GetType();
            //根据反射从传递进来的属性名信息得到要显示的属性 
            List<System.Reflection.PropertyInfo> myPro = new List<System.Reflection.PropertyInfo>();
            foreach (string cName in columnInfo.Keys)
            {
                System.Reflection.PropertyInfo p = myType.GetProperty(cName);
                if (p != null)
                {
                    myPro.Add(p);
                    excelStr += columnInfo[cName] + "\t";
                }
            }
            //如果没有找到可用的属性则结束 
            if (myPro.Count == 0) { return; }
            excelStr += "\n";
            foreach (T obj in objList)
            {
                foreach (System.Reflection.PropertyInfo p in myPro)
                {
                    excelStr += p.GetValue(obj, null) + "\t";
                }
                excelStr += "\n";
            }
            //输出EXCEL 
            HttpResponse rs = System.Web.HttpContext.Current.Response;
            rs.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");
            rs.AppendHeader("Content-Disposition", "attachment;filename=" + FileName);
            rs.ContentType = "application/ms-excel";
            rs.Write(excelStr);
            rs.End();
        }
        /// <summary>
        /// 将一组字典导出成Excel
        /// </summary>
        /// <param name="objList"></param>
        /// <param name="FileName"></param>
        /// <param name="columnInfo"></param>
        public static void ExExcel(List<Dictionary<string, object>> objList, string FileName, Dictionary<string, string> columnInfo)
        {
            if (columnInfo.Count == 0) { return; }
            if (objList.Count == 0) { return; }
            //生成EXCEL的HTML 
            string excelStr = "";
            Dictionary<string, object> dic = objList[0];
            foreach (string cName in columnInfo.Keys)
            {
                excelStr += columnInfo[cName] + "\t";
            }
            //如果没有找到可用的属性则结束 
            //if (myPro.Count == 0) { return; }
            excelStr += "\n";
            foreach (Dictionary<string,object> obj in objList)
            {
                foreach (string key in columnInfo.Keys)
                {
                    if (obj.ContainsKey(key))
                        excelStr += obj[key] + "\t";
                    else excelStr += "\t";
                }
                excelStr += "\n";
            }
            //输出EXCEL 
            HttpResponse rs = System.Web.HttpContext.Current.Response;
            rs.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");
            rs.AppendHeader("Content-Disposition", "attachment;filename=" + FileName);
            rs.ContentType = "application/ms-excel";
            rs.Write(excelStr);
            rs.End();
        }
    }
}
