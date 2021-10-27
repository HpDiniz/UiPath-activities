using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using Elogroup.BigQuery.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using Newtonsoft.Json.Linq;
using Google.Apis.Bigquery.v2.Data;
using Google.Cloud.BigQuery.V2;
using Google.Apis.Auth.OAuth2;

namespace Elogroup.BigQuery.Activities
{
    [LocalizedDisplayName(nameof(Resources.RealizarConsulta_DisplayName))]
    [LocalizedDescription(nameof(Resources.RealizarConsulta_Description))]
    public class RealizarConsulta : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.RealizarConsulta_Credentials_DisplayName))]
        [LocalizedDescription(nameof(Resources.RealizarConsulta_Credentials_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> Credentials { get; set; }

        [LocalizedDisplayName(nameof(Resources.RealizarConsulta_Query_DisplayName))]
        [LocalizedDescription(nameof(Resources.RealizarConsulta_Query_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> Query { get; set; }

        [LocalizedDisplayName(nameof(Resources.RealizarConsulta_Result_DisplayName))]
        [LocalizedDescription(nameof(Resources.RealizarConsulta_Result_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<DataTable> Result { get; set; }

        #endregion


        #region Constructors

        public RealizarConsulta()
        {
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (Credentials == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(Credentials)));
            if (Query == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(Query)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Inputs
            var credentialspath = Credentials.Get(context);
            var query = Query.Get(context);

            GoogleCredential credentials = GoogleCredential.FromFile(credentialspath);

            JObject jsonData = JObject.Parse(File.ReadAllText(credentialspath));

            BigQueryClient client = BigQueryClient.Create(jsonData.GetValue("project_id").ToString(), credentials);

            BigQueryJob job = client.CreateQueryJob(
                sql: query,
                parameters: null,
                options: new QueryOptions { UseQueryCache = false }
            );

            job = job.PollUntilCompleted().ThrowOnAnyError();

            BigQueryResults results = client.GetQueryResults(job.Reference);

            DataTable DTblBigQuery = new DataTable();

            if (results != null && results.Schema != null)
            {
                foreach (TableFieldSchema col in results.Schema.Fields)
                {
                    DTblBigQuery.Columns.Add(col.Name.ToString(), typeof(string));
                }

                foreach (BigQueryRow row in results)
                {
                    DataRow newRow = DTblBigQuery.NewRow();

                    foreach (TableFieldSchema col in results.Schema.Fields)
                    {
                        if (row[col.Name.ToString()] != null)
                        {
                            newRow[col.Name.ToString()] = row[col.Name.ToString()].ToString();
                        }
                    }

                    DTblBigQuery.Rows.Add(newRow);
                }
            }

            // Outputs
            return (ctx) => {
                Result.Set(ctx, DTblBigQuery);
            };
        }

        #endregion
    }
}

