using System;
using MyOnlineMemo.Util;

namespace MyOnlineMemo.BusinessLayer
{
    public class AuthService
    {
        public AuthService()
        {
        }
        public bool CreateAccount(string usrId, string psswrd)
        {
            var sql = "INSERT INTO MOMM_USR(USR_ID, PSSWRD, TSVFLG) VALUES(@USR_ID, @PSSWRD, '1')";
            var param = new { USR_ID = usrId, PSSWRD = psswrd };
            var dataAccesser = new DataAccesser();
            //dataAccesser.BeginTran();
            dataAccesser.ExecuteNonQuery(sql, param);
            //dataAccesser.Commit();
            return true;
        }
    }
}
