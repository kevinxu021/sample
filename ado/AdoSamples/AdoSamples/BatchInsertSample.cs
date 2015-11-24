﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EsgynDB.Data;

namespace AdoSamples
{
    [TestClass]
    public class BatchInsertSample
    {
        private string ConnectionString = "server=10.10.10.136:23400;user=zz;password=zz;schema=ado";
        private log4net.ILog log = log4net.LogManager.GetLogger(typeof(BatchInsertSample));

        [TestMethod]
        public void TestBatchLoad()
        {
            EsgynDBConnection conn = new EsgynDBConnection();
            conn.ConnectionString = ConnectionString;
            conn.Open();

            EsgynDBCommand cmd = conn.CreateCommand();
            prepareTable(cmd);

            //start loading...
            log.Info("Start loading...");
            cmd.CommandText = "upsert using load into t0 values(?,?)";
            cmd.Parameters.Add(new EsgynDBParameter("c0", EsgynDBType.Varchar));
            cmd.Parameters.Add(new EsgynDBParameter("c1", EsgynDBType.Varchar));

            log.Info("Prepare the insertion sql");
            cmd.Prepare();

            log.Info("Put rows into batch");
            for (int i = 0; i < 10; i++)
            {
                cmd.Parameters[0].Value = "test col-" + i;
                cmd.Parameters[1].Value = "test col " + (i * 2 + 1);
                cmd.AddBatch();
            }

            log.Info("Execute batch");
            cmd.ExecuteNonQuery();
            
            //Check all inserted data
            log.Info("Check all inserted data");
            cmd.Parameters.Clear();
            cmd.CommandText = "select * from t0";
            EsgynDBDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    log.Info(dr.GetValue(i) + " " + dr.GetDataTypeName(i));
                }

            }

        }

        private void prepareTable(EsgynDBCommand cmd)
        {
            try
            {
                log.Info("log4net: prepare schema and table");
                cmd.CommandText = "create schema if not exists ado";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "drop table if exists t0";
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
            }

            cmd.CommandText = "create table t0 (c1 varchar(20), c2 nchar(20)) no partition";
            cmd.ExecuteNonQuery();
        }
    }
}
