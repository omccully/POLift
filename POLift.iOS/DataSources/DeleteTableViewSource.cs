using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Foundation;
using UIKit;

namespace POLift.iOS.DataSources
{
    public class DeleteTableViewSource<T> : UITableViewSource
    {
        //Action<T, Action>
        public event Action<T, Action>  DeleteClicked;

        public event EventHandler<T> RowClicked;
        
        protected IList<T> Data;

        public readonly string CellId;

        public DeleteTableViewSource(IList<T> data)
        {
            this.Data = data;

            CellId = GetCellId<T>();
        }

        public static string GetCellId<Ts>()
        {
            return $"DeleteTableViewSource<{typeof(Ts).Name}>.CellId";
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Data.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(CellId);

            cell.TextLabel.Text = GetTextLabelText(indexPath);

            return cell;
        }

        protected virtual string GetTextLabelText(NSIndexPath indexPath)
        {
            return Data[indexPath.Row].ToString();
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            RowClicked?.Invoke(this, Data[indexPath.Row]);
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }

        public override void CommitEditingStyle(UITableView tableView,
            UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            if (editingStyle == UITableViewCellEditingStyle.Delete)
            {
                if (DeleteClicked != null)
                {
                    int start_count = Data.Count;
                    T item = Data[indexPath.Row];
                    DeleteClicked(item, delegate
                    {
                        // ensure this delegate isn't called twice
                        // for when multiple event handlers are hooked up 
                        if (start_count == Data.Count)
                        {
                          
                            Data.RemoveAt(indexPath.Row);

                            tableView.DeleteRows(new NSIndexPath[] { indexPath },
                                UITableViewRowAnimation.Fade);
                        }
                    });
                }
            }
        }
    }
}