using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Foundation;
using UIKit;

using StoreKit;

using POLift.Core.Service;
using System.Threading.Tasks;

namespace POLift.iOS.Service
{
    public class StoreKitLicenseManager : LicenseManager 
    {
        public StoreKitLicenseManager(string device_id, KeyValueStorage kvs = null)
            : base(device_id, kvs)
        {

        }

        public Task<bool> RestoreLicense()
        {
            EventWaitHandle waitHandle = new AutoResetEvent(false);
            LicensePurchaseManager lrm = new LicensePurchaseManager(ProductID);

            lrm.RestoreLicense += delegate
            {
                waitHandle.Set();
            };

            lrm.Restore();

            return Task.Run<bool>(delegate
            {
                LicensePurchaseManager lrm_safe = lrm;
                return waitHandle.WaitOne(5000);
            });
        }

        protected override Task<bool> CheckLicenseStrict_NotCached()
        {
            // don't check app store every time app is launched.
            return Task.Run<bool>(delegate
            {
                throw new Exception();
                return false;
            });
        }

        public override Task<bool> PromptToBuyLicense()
        {
            System.Diagnostics.Debug.WriteLine("Sklm.PromptToBuyLicense");
            EventWaitHandle waitHandle = new AutoResetEvent(false);

            LicensePurchaseManager lmp = new LicensePurchaseManager(ProductID);

            bool success = false;

            lmp.PurchaseFailed += delegate
            {
                System.Diagnostics.Debug.WriteLine("Sklm.PurchaseFailed");
                waitHandle.Set();
            };

            lmp.PurchaseSuccess += delegate
            {
                System.Diagnostics.Debug.WriteLine("Sklm.PurchaseSuccess");
                success = true;
                waitHandle.Set();
            };

            //lmp.PurchaseLicense();

            //NSString prod_id_str = new NSString(ProductID);

            NSString[] prod_id_arr = new NSString[] { new NSString("polift_license"),
                new NSString("com.cml.POLift.license"),  new NSString("license") };
            NSSet productIdentifiers = NSSet.MakeNSObjectSet<NSString>(prod_id_arr);

            SKProductsRequest req = new SKProductsRequest(productIdentifiers);
            req.Delegate = new Test();
            req.Start();

            
            return Task.Run<bool>(() => false);

            /*return Task.Run<bool>(delegate
            {
                LicensePurchaseManager lmp_safe = lmp;
                System.Diagnostics.Debug.WriteLine("waiting for purchase response...");
                if (!waitHandle.WaitOne(60000))
                {
                    System.Diagnostics.Debug.WriteLine("purchase timed out");
                    return false;
                }
                System.Diagnostics.Debug.WriteLine("Purchase success = " + success);
                return success;
            });*/
        }

        class Test : NSObject, ISKProductsRequestDelegate
        {
            public void ReceivedResponse(SKProductsRequest request, SKProductsResponse response)
            {
                Console.WriteLine(request.DebugDescription);
                //Console.WriteLine(request.);

                foreach(string invalid in response.InvalidProducts)
                {
                    Console.WriteLine("invalid product: " + invalid);
                }
                
                Console.WriteLine("StoreKitLicenseManager.ReceivedResponse");
                foreach (SKProduct prod in response.Products)
                {
                    Console.WriteLine("product " + prod.DebugDescription);
                }
                Console.WriteLine("END products");
            }
        }

        private class LicensePurchaseManager : PurchaseManager
        {
            public event EventHandler PurchaseFailed;
            public event EventHandler PurchaseSuccess;

            string productId;
            public LicensePurchaseManager(string productId)
            {
                this.productId = productId;
            }


            /* public void RequestProductData()
             {
                 NSSet productIdentifiers = NSSet.MakeNSObjectSet<NSString>(
                     new NSString[] { new NSString(ProductID) });​​​
                 SKProductsRequest rq = new SKProductsRequest(productIdentifiers);
                 rq.Delegate = this;
                 rq.Start();
             }*/

            public void PurchaseLicense()
            {
                base.PurchaseProduct(productId);
            }


            protected override void CompleteTransaction(string productId)
            {
                System.Diagnostics.Debug.WriteLine("CompleteTransaction(" + productId);

                if (productId != this.productId)
                {
                    throw new InvalidOperationException(productId + " != " + this.productId);
                }

                PurchaseSuccess?.Invoke(this, null);
            }

            public override void RequestFailed(SKRequest request, NSError error)
            {
                System.Diagnostics.Debug.WriteLine("RequestFailed");
                PurchaseFailed?.Invoke(this, null);

                base.RequestFailed(request, error);
            }



            public event EventHandler RestoreLicense;

            protected override void RestoreTransaction(string productId)
            {
                System.Diagnostics.Debug.WriteLine("RestoreTransaction(" + productId);
                if (productId == this.productId)
                {
                    RestoreLicense?.Invoke(this, null);
                }
            }
        }

        abstract class PurchaseManager : SKProductsRequestDelegate
        {
            public static readonly NSString InAppPurchaseManagerProductsFetchedNotification = new NSString("InAppPurchaseManagerProductsFetchedNotification");
            public static readonly NSString InAppPurchaseManagerTransactionFailedNotification = new NSString("InAppPurchaseManagerTransactionFailedNotification");
            public static readonly NSString InAppPurchaseManagerTransactionSucceededNotification = new NSString("InAppPurchaseManagerTransactionSucceededNotification");
            public static readonly NSString InAppPurchaseManagerRequestFailedNotification = new NSString("InAppPurchaseManagerRequestFailedNotification");

            protected SKProductsRequest ProductsRequest { get; set; }

            public PurchaseManager()
            {
                CustomPaymentObserver theObserver = new CustomPaymentObserver(this);
                SKPaymentQueue.DefaultQueue.AddTransactionObserver(theObserver);
            }

            // Verify that the iTunes account can make this purchase for this application
            public bool CanMakePayments()
            {
                return SKPaymentQueue.CanMakePayments;
            }

            // request multiple products at once
            public void RequestProductData(List<string> productIds)
            {
                NSString[] array = productIds.Select(pId => (NSString)pId).ToArray();
                NSSet productIdentifiers = NSSet.MakeNSObjectSet<NSString>(array);

                //set up product request for in-app purchase
                ProductsRequest = new SKProductsRequest(productIdentifiers);
                ProductsRequest.Delegate = this; // SKProductsRequestDelegate.ReceivedResponse
                ProductsRequest.Start();
            }

            // received response to RequestProductData - with price,title,description info
            public override void ReceivedResponse(SKProductsRequest request, SKProductsResponse response)
            {
                SKProduct[] products = response.Products;

                NSMutableDictionary userInfo = new NSMutableDictionary();
                for (int i = 0; i < products.Length; i++)
                    userInfo.Add((NSString)products[i].ProductIdentifier, products[i]);
                NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerProductsFetchedNotification, this, userInfo);

                foreach (string invalidProductId in response.InvalidProducts)
                    Console.WriteLine("Invalid product id: {0}", invalidProductId);
            }

            public void PurchaseProduct(string appStoreProductId)
            {
                Console.WriteLine("PurchaseProduct {0}", appStoreProductId);
                SKPayment payment = SKPayment.PaymentWithProduct(appStoreProductId);
                SKPaymentQueue.DefaultQueue.AddPayment(payment);
            }

            public void FailedTransaction(SKPaymentTransaction transaction)
            {
                //SKErrorPaymentCancelled == 2
                string errorDescription = transaction.Error.Code == 2 ? "User CANCELLED FailedTransaction" : "FailedTransaction";
                Console.WriteLine("{0} Code={1} {2}", errorDescription, transaction.Error.Code, transaction.Error.LocalizedDescription);

                FinishTransaction(transaction, false);
            }

            public void CompleteTransaction(SKPaymentTransaction transaction)
            {
                Console.WriteLine("CompleteTransaction {0}", transaction.TransactionIdentifier);
                string productId = transaction.Payment.ProductIdentifier;

                // Register the purchase, so it is remembered for next time
                CompleteTransaction(productId);
                FinishTransaction(transaction, true);
            }

            protected virtual void CompleteTransaction(string productId)
            {

            }

            public void FinishTransaction(SKPaymentTransaction transaction, bool wasSuccessful)
            {
                Console.WriteLine("FinishTransaction {0}", wasSuccessful);
                // remove the transaction from the payment queue.
                SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);     // THIS IS IMPORTANT - LET'S APPLE KNOW WE'RE DONE !!!!

                NSDictionary userInfo = new NSDictionary("transaction", transaction);
                var notificationKey = wasSuccessful ? InAppPurchaseManagerTransactionSucceededNotification : InAppPurchaseManagerTransactionFailedNotification;
                NSNotificationCenter.DefaultCenter.PostNotificationName(notificationKey, this, userInfo);
            }

            /// <summary>
            /// Probably could not connect to the App Store (network unavailable?)
            /// </summary>
            public override void RequestFailed(SKRequest request, NSError error)
            {
                Console.WriteLine(" ** RequestFailed ** {0}", error.LocalizedDescription);

                // send out a notification for the failed transaction
                NSDictionary userInfo = new NSDictionary("error", error);
                NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerRequestFailedNotification, this, userInfo);
            }

            /// <summary>
            /// Restore any transactions that occurred for this Apple ID, either on
            /// this device or any other logged in with that account.
            /// </summary>
            public void Restore()
            {
                Console.WriteLine(" ** Restore **");
                // theObserver will be notified of when the restored transactions start arriving <- AppStore
                SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions();
            }

            public virtual void RestoreTransaction(SKPaymentTransaction transaction)
            {
                // Restored Transactions always have an 'original transaction' attached
                Console.WriteLine("RestoreTransaction {0}; OriginalTransaction {1}", transaction.TransactionIdentifier, transaction.OriginalTransaction.TransactionIdentifier);
                string productId = transaction.OriginalTransaction.Payment.ProductIdentifier;
                // Register the purchase, so it is remembered for next time
                RestoreTransaction(productId);
                FinishTransaction(transaction, true);
            }

            protected virtual void RestoreTransaction(string productId)
            {

            }


            class CustomPaymentObserver : SKPaymentTransactionObserver
            {
                private PurchaseManager theManager;

                public CustomPaymentObserver(PurchaseManager manager)
                {
                    theManager = manager;
                }

                public override void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
                {
                    Console.WriteLine("UpdatedTransactions");
                    foreach (SKPaymentTransaction transaction in transactions)
                    {
                        switch (transaction.TransactionState)
                        {
                            case SKPaymentTransactionState.Purchased:
                                theManager.CompleteTransaction(transaction);
                                break;
                            case SKPaymentTransactionState.Failed:
                                theManager.FailedTransaction(transaction);
                                break;
                            case SKPaymentTransactionState.Restored:
                                theManager.RestoreTransaction(transaction);
                                break;
                            default:
                                break;
                        }
                    }
                }

                public override void PaymentQueueRestoreCompletedTransactionsFinished(SKPaymentQueue queue)
                {
                    // Restore succeeded
                    Console.WriteLine(" ** RESTORE PaymentQueueRestoreCompletedTransactionsFinished ");
                }

                public override void RestoreCompletedTransactionsFailedWithError(SKPaymentQueue queue, NSError error)
                {
                    // Restore failed somewhere...
                    Console.WriteLine(" ** RESTORE RestoreCompletedTransactionsFailedWithError " + error.LocalizedDescription);
                }
            }
        }
    }
}