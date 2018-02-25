using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Dapper;

namespace MyOnlineMemo.Util
{
    public class DataAccesser
    {
        private string _conStr;
        private SqlConnection _conn;
        private bool _autoCommit;
        private bool _beginTran;
        private SqlTransaction _tran;
        public DataAccesser()
        {
            _conStr = "";
            _autoCommit = true;
            _beginTran = false;
            _tran = null;
            _conn = new SqlConnection();
            _conn.Open();
        }
        ~DataAccesser()
        {
            _conn.Close();
            if (_autoCommit == false && _tran != null)
            {
                Rollback();
                throw new Exception("TRANSACRION is not closed!! ROLLBACK is done.");
            }
        }
        public void BeginTran()
        {
            _autoCommit = false;
            _tran = _conn.BeginTransaction();
        }
        public void Commit()
        {
            this.CheckTran();
            _tran.Commit();
            _tran = null;
        }
        public void Rollback()
        {
            this.CheckTran();
            _tran.Rollback();
            _tran = null;
        }

        /*
        * sqlでバインド変数を使用する場合は、@マークを使用します。
        *  ex) select name from syain where syain_code = @SYAIN_CODE
        * 第二引数にはsqlにバインドする値をセットします。
        * モデルクラスや匿名型を渡すことでDapperがsqlにバインドします。
        *  ex) var param = new {SYAIN_CODE = "1001"} <-匿名型
        */
        public int ExecuteNonQuery(string sql, dynamic param)
        {
            int execCnt;
            if (_autoCommit)
            {
                using (var tran = _conn.BeginTransaction())
                {
                    try
                    {
                        execCnt = _conn.Execute(sql, (object)param, tran);
                        tran.Commit();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw e;
                    }
                }
            }
            else
            {
                try
                {
                    execCnt = _conn.Execute(sql, (object)param, _tran);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return execCnt;
        }

        public T QueryForModel<T>(string sql, dynamic param)
        {
            try
            {
                return _conn.Query<T>(sql, (object)param).FirstOrDefault();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<T> QueryForListModel<T>(string sql, dynamic param)
        {
            try
            {
                return _conn.Query<T>(sql, (object)param);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #region private
        private void CheckTran()
        {
            if (_autoCommit == false || _tran == null)
            {
                throw new Exception("There is no TRANSACTION!!");
            }
        }
        #endregion
    }
}
