using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;


public partial class StoredProcedures
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventNameVar">Pass null value for eventNameVar if event name is not specified.</param>
    /// <param name="procIDVar"></param>
    /// <param name="DescriptionVar"></param>
    /// <param name="IDAuctionSiteVar"></param>
    /// <param name="IDAuctionEventVar"></param>
    /// <param name="requestIDVar"></param>
    /// <param name="numUnitsVar"></param>
    /// <param name="failedScrape"></param>
    /// <param name="noteVar"></param>
    /// <param name="xmlTicket"></param>
    /// <param name="IDLogVar"></param>
    /// <param name="serverNameVar"></param>
    /// <param name="dbNameVar"></param>
    /// <param name="objectName"></param>
    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void spLogEvent(
        String eventNameVar,
        int procIDVar,
        String descriptionVar,
        int IDAuctionSiteVar,
        int IDAuctionEventVar,
        int requestIDVar,
        int numUnitsVar,
        Boolean failedScrapeVar,
        String noteVar,
        String xmlTicketVar,
        ref int IDLogVar,
        String serverNameVar,
        String dbNameVar,
        String objectNameVar
        )
    {
        eventNameVar = eventNameVar ?? "Unknown Event";

        String auctionVar = "";

        eventNameVar = (dbNameVar ?? m.mConn.Database.ToString()) + ".dbo." + objectNameVar;

        try
        {
            if (IDAuctionSiteVar == null)
            {
                m.mConn.Open();
                m.mCmd.CommandText = @"SELECT IDAuctionSite   
		                                FROM dbo.tblAuctionEvent WITH (NOLOCK)
		                                WHERE IDAuctionEvent = " + IDAuctionEventVar;
                IDAuctionEventVar = int.Parse(m.mCmd.ExecuteScalar().ToString());
                m.mConn.Close();
            }
        }
        catch (FormatException ex)
        {
            m.mP.Send(ex.ToString());
        }

        m.mConn.Open();
        m.mCmd.CommandText = @"SELECT [Description]
	                            FROM dbo.tblAuctionSite WITH (NOLOCK)
	                            WHERE IDAuctionSite = " + IDAuctionSiteVar;
        auctionVar = m.mCmd.ExecuteScalar().ToString();
        m.mConn.Close();
        
        m.mP.Send("============= " +
                    "spLogEvent" +
                    "IDAucationSiteVar = '" + ((IDAuctionSiteVar+"") ?? "null") + @"',
                    IDAuctionEventVar = '" + ((IDAuctionEventVar + "") ?? "null") + @"',
                    eventNameVar =  " + eventNameVar + @"',
                    descriptionVar =  " + descriptionVar + @"',
                    requestIDVar =  " + requestIDVar + @"',
                    numUnitsVar =  " + numUnitsVar + @"',
                    failedScrapeVar =  " + failedScrapeVar + @"',
                    noteVar =  " + noteVar + @"',
                    IDLogVar =  " + IDLogVar + @"',
                    xmlTicketVar =  " + xmlTicketVar + @"',    
                    =============");

        if (IDLogVar < 1 || IDLogVar == null)
        {
            m.mConn.Open();
            m.mCmd.CommandText = @"INSERT INTO tblLogEvent (  			
			                            EventName,
			                            [Description],
			                            IdAuctionSite,
			                            IdAuctionEvent,
			                            RequestId,
			                            Auction,
			                            NoOfUnits,
			                            Note,
			                            XMLTicket,
			                            ServerName,
			                            DatabaseName)
		                            VALUES (
			                            '" + eventNameVar + @"','"
                                        + descriptionVar + @"',"
                                        + IDAuctionSiteVar + @","
                                        + IDAuctionEventVar + @","
                                        + requestIDVar + @",'"
                                        + auctionVar + @"',"
                                        + numUnitsVar + @",'"
                                        + noteVar + @"','"
                                        + xmlTicketVar + @"','"
                                        + serverNameVar + @"','"
                                        + dbNameVar + @"')";
            m.mCmd.ExecuteNonQuery();

            m.mCmd.CommandText = "SELECT @@SCOPE_IDENTITY()";
            try
            {
                @IDLogVar = Convert.ToInt32(m.mCmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                m.mP.Send(ex.ToString());
            }

            m.mConn.Close();
        }
        else {

            m.mConn.Open();

            m.mCmd.CommandText = @"UPDATE tblLogEvent
			                            SET DateAdded = GETDATE(),
				                            EventName = '" + eventNameVar + @"',
				                            [Description] = '" + descriptionVar + @"',
				                            IdAuctionSite = " + IDAuctionSiteVar + @",
				                            IdAuctionEvent = " + IDAuctionEventVar + @",
				                            RequestId = " + requestIDVar + @",
				                            Auction = '" + auctionVar + @"',
				                            NoOfUnits = " + numUnitsVar+ @",
				                            Note = '" + noteVar + @"',
				                            XMLTicket = '" + xmlTicketVar + @"',
				                            ServerName = '" + serverNameVar + @"',
				                            DatabaseName = '" + dbNameVar + @"',
		                            WHERE IdLogEvent = " + IDLogVar + @",";
            m.mCmd.ExecuteNonQuery();
            //m.mP.Send(m.mCmd.ExecuteNonQuery()+"");
            
            m.mConn.Close();
        }
    }
};
