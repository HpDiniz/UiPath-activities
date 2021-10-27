using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using Elogroup.BigQuery.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using UiPath.Shared.Activities.Utilities;
using Google.Cloud.BigQuery.V2;
using Google.Apis.Bigquery.v2.Data;

namespace Elogroup.BigQuery.Activities
{
    [LocalizedDisplayName(nameof(Resources.ExecuteQuery_DisplayName))]
    [LocalizedDescription(nameof(Resources.ExecuteQuery_Description))]
    public class ExecuteQuery : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.Timeout_DisplayName))]
        [LocalizedDescription(nameof(Resources.Timeout_Description))]
        public InArgument<int> TimeoutMS { get; set; } = 60000;

        [LocalizedDisplayName(nameof(Resources.ExecuteQuery_Query_DisplayName))]
        [LocalizedDescription(nameof(Resources.ExecuteQuery_Query_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> Query { get; set; }

        [LocalizedDisplayName(nameof(Resources.ExecuteQuery_DataTable_DisplayName))]
        [LocalizedDescription(nameof(Resources.ExecuteQuery_DataTable_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<DataTable> DataTable { get; set; }

        #endregion


        #region Constructors

        public ExecuteQuery()
        {
            Constraints.Add(ActivityConstraints.HasParentType<ExecuteQuery, BigQueryScope>(string.Format(Resources.ValidationScope_Error, Resources.BigQueryScope_DisplayName)));
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (Query == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(Query)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Object Container: Use objectContainer.Get<T>() to retrieve objects from the scope
            var objectContainer = context.GetFromContext<IObjectContainer>(BigQueryScope.ParentContainerPropertyTag);

            // Inputs
            var timeout = TimeoutMS.Get(context);

            Task task = new Task(() =>
            {

                var query = Query.Get(context);

                var property = context.DataContext.GetProperties()[BigQueryScope.ParentContainerPropertyTag];
                var container = property.GetValue(context.DataContext) as IObjectContainer;
                BigQueryClient client = container.Get<BigQueryClient>();

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

                    DataTable.Set(context, DTblBigQuery);
                }

            });
   
            task.Start();
            task.Wait(timeout); // Set a timeout on the execution
            if (!task.IsCompleted)
            {
                throw new TimeoutException(Resources.Timeout_Error);
            }

            // Outputs
            return (ctx) => {
                DataTable.Get(ctx);
            };
        }

        #endregion
    }
}