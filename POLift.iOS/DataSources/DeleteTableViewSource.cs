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
        
        public IList<T> Data;

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

        public T DataFromIndexPath(NSIndexPath indexPath)
        {
            return Data[indexPath.Row];
        }

        protected virtual string GetTextLabelText(NSIndexPath indexPath)
        {
            return DataFromIndexPath(indexPath).ToString();
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            Console.WriteLine("DeleteTableViewSource.RowSelected");
            RowClicked?.Invoke(this, Data[indexPath.Row]);
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }

        public override void CommitEditingStyle(UITableView tableView,
            UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            System.Diagnostics.Debug.WriteLine("CommitEditingStyle");
            if (editingStyle == UITableViewCellEditingStyle.Delete)
            {
                Console.WriteLine("click 1");
                if (DeleteClicked != null)
                {
                    Console.WriteLine("click 2");
                    int start_count = Data.Count;
                    T item = Data[indexPath.Row];
                    DeleteClicked(item, delegate
                    {
                        Console.WriteLine("callback a");
                        // ensure this delegate isn't called twice
                        // for when multiple event handlers are hooked up 
                        if (start_count == Data.Count)
                        {
                            Console.WriteLine("callback b");
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