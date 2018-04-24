using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using CoreGraphics;

namespace POLift.iOS.Service
{
    class LongPressTableViewCellMover
    {
        public UILongPressGestureRecognizer GestureRecognizer { get; set; }

        UITableView table_view;
        public LongPressTableViewCellMover(UITableView table_view)
        {
            this.table_view = table_view;
            GestureRecognizer =
                new UILongPressGestureRecognizer(GestureRecognized);
        }

        NSIndexPath last_valid_path = null;
        NSIndexPath source_index_path = null;
        UIView snapshot = null;

        void GestureRecognized(UILongPressGestureRecognizer lp)
        {
            UIGestureRecognizerState state = lp.State;

            CGPoint location = lp.LocationInView(table_view);

            NSIndexPath path = table_view.IndexPathForRowAtPoint(location);
            if (path != null) last_valid_path = path;

            Console.WriteLine("LONG PRESS DETECTED: " + state + " " + location.ToString() + " " + path?.DebugDescription);

            CGPoint center;
            UITableViewCell cell;

            switch (state)
            {
                case UIGestureRecognizerState.Began:
                    if (path == null) break;
                    source_index_path = path;

                    cell = table_view.CellAt(path);

                    // create snapshot centered at cell
                    snapshot = cell.CustomSnapshotFromView(); //cell.SnapshotView(false);
                    center = cell.Center;
                    snapshot.Center = center;
                    snapshot.Alpha = new nfloat(0.0);
                    table_view.AddSubview(snapshot);

                    UIView.Animate(0.25, delegate
                    {
                        // make snapshot bigger
                        center.Y = location.Y;
                        snapshot.Center = center;
                        snapshot.Transform = CGAffineTransform.MakeScale(new nfloat(1.05), new nfloat(1.05));
                        snapshot.Alpha = new nfloat(0.98);

                        // hide cell
                        cell.Alpha = new nfloat(0.0);
                    }, delegate
                    {
                        cell.Hidden = true; // hide cell
                    });
                    break;
                case UIGestureRecognizerState.Changed:
                    if (path == null) break;
                    if (snapshot == null) break;
                    center = snapshot.Center;
                    center.Y = location.Y;
                    snapshot.Center = center;
                    break;
                default:
                    //if (path == null) break;
                    if (source_index_path == null) break;
                    if (snapshot == null) break;

                    // remove snapshot view and make old cell visible again
                    cell = table_view.CellAt(source_index_path);
                    cell.Hidden = false;
                    cell.Alpha = new nfloat(0.0);

                    // move the rows. 
                    if (last_valid_path != null && !last_valid_path.IsEqual(source_index_path))
                    {
                        if(table_view.Source != null)
                        {
                            table_view.Source.MoveRow(table_view, source_index_path, last_valid_path);
                        }
                        else if(table_view.DataSource != null)
                        {
                            table_view.DataSource.MoveRow(table_view, source_index_path, last_valid_path);
                        }
                        else
                        {
                            Console.WriteLine($"No data source for move ({source_index_path.DebugDescription} -> {last_valid_path.DebugDescription}");
                        }
                    }

                    UIView.Animate(0.25, delegate
                    {
                        snapshot.Center = cell.Center;
                        snapshot.Transform = CGAffineTransform.MakeIdentity();
                        snapshot.Alpha = new nfloat(0.0);

                        cell.Alpha = new nfloat(1.0);
                    }, delegate
                    {
                        snapshot.RemoveFromSuperview();
                        snapshot = null;

                        source_index_path = null;
                    });
                    break;
            }
        }
    }
}