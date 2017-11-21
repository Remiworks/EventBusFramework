using AttributeLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitmqReceiver
{
    [EventListener("depQueue")]
    public class UserEventListener
    {
        private readonly Mather _mather;

        public UserEventListener(Mather mather)
        {
            Console.WriteLine("Constructor: ");
            _mather = mather;
        }

        [Topic("user.event.created")]
        public void UserCreated(UserModel testModel)
        {
            Console.WriteLine("UserModel: " + testModel.Name);
            _mather.Calculate();
        }
    }
}
