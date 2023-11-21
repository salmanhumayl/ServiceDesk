using Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace AJCCFM.Models
{
    public class ClaimCart
    {
       
        string shoppingCartId { get; set; }
        public const string CartSessionKey = "CartId";


        public static ClaimCart GetCart(HttpContextBase context)
        {
            var cart = new ClaimCart();
            cart.shoppingCartId = cart.GetCartId(context);
            return cart;
        }

        // We're using HttpContextBase to allow access to cookies.
        public String GetCartId(HttpContextBase context)
        {


            if (context.Session[CartSessionKey] == null)
            {


                if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                {
                    // User is logged in, associate the cart with there username
                    context.Session[CartSessionKey] = context.User.Identity.Name;
                }
                else
                {
                    // Generate a new random GUID using System.Guid Class
                    Guid tempCartId = Guid.NewGuid();

                    // Send tempCartId back to client as a cookie
                    context.Session[CartSessionKey] = tempCartId.ToString();
                }
            }
            return context.Session[CartSessionKey].ToString();
        }

        public void AddToClaim(Cart Carts)
        {

           
        }

        

        
        }
        
       


       

      

     
    }
