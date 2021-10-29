using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Generic;
using Elogroup.Utilitarios.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;

namespace Elogroup.Utilitarios.Activities
{
    [LocalizedDisplayName(nameof(Resources.ConverterDataTableParaHTML_DisplayName))]
    [LocalizedDescription(nameof(Resources.ConverterDataTableParaHTML_Description))]
    public class ConverterDataTableParaHTML : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.ConverterDataTableParaHTML_DataTable_DisplayName))]
        [LocalizedDescription(nameof(Resources.ConverterDataTableParaHTML_DataTable_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<DataTable> DataTable { get; set; }

        [LocalizedDisplayName(nameof(Resources.ConverterDataTableParaHTML_Options_DisplayName))]
        [LocalizedDescription(nameof(Resources.ConverterDataTableParaHTML_Options_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<object> Options { get; set; }

        [LocalizedDisplayName(nameof(Resources.ConverterDataTableParaHTML_Result_DisplayName))]
        [LocalizedDescription(nameof(Resources.ConverterDataTableParaHTML_Result_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> Result { get; set; }

        #endregion


        #region Constructors

        public ConverterDataTableParaHTML()
        {
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (DataTable == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(DataTable)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Inputs
            var datatable = DataTable.Get(context);
            var options = Options.Get(context);

            Dictionary<string, string[]> dictionary = (Dictionary<string, string[]>)options;

            // HEADERS
            string headers = String.Empty;
            foreach (DataColumn col in datatable.Columns)
            {
                string columnName = col.ColumnName.ToString();
                try
                {
                    string columnAlias = dictionary[columnName][0].ToString();
                    string columnStyle = dictionary[columnName][1].ToString();
                    headers += String.Format("<th style='{1}'>{0}</th>", columnAlias, columnStyle);
                }
                catch (Exception e)
                {
                    headers += String.Format("<th>{0}</th>", columnName);
                }
            }
            // ROWS
            string rows = String.Empty;
            foreach (DataRow row in datatable.Rows)
            {
                string cells = String.Empty;
                foreach (DataColumn col in datatable.Columns)
                {
                    string columnName = col.ColumnName.ToString();
                    string cellValue = row[columnName].ToString();
                    try
                    {
                        string cellStyle = dictionary[col.ColumnName][2].ToString();
                        cells += String.Format("<td style='{1}'>{0}</td>", cellValue, cellStyle);
                    }
                    catch (Exception e)
                    {
                        cells += String.Format("<td>{0}</td>", cellValue);
                    }
                }
                rows += String.Format("<tr>{0}</tr>", cells);
            }
            // EXPORT 
            string table = "<table border='1'><thead><tr>{0}</tr></thead><tbody>{1}</tbody></table>";

            // Outputs
            return (ctx) => {
                Result.Set(ctx, String.Format(table, headers, rows));
            };
        }

        #endregion
    }
}

