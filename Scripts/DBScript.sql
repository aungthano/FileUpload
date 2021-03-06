USE [master]
GO
/****** Object:  Database [FileUpload]    Script Date: 5/3/2022 1:33:41 AM ******/
CREATE DATABASE [FileUpload]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'FileUpload', FILENAME = N'D:\Dev\Database\FileUpload.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'FileUpload_log', FILENAME = N'D:\Dev\Database\FileUpload_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [FileUpload] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [FileUpload].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [FileUpload] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [FileUpload] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [FileUpload] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [FileUpload] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [FileUpload] SET ARITHABORT OFF 
GO
ALTER DATABASE [FileUpload] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [FileUpload] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [FileUpload] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [FileUpload] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [FileUpload] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [FileUpload] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [FileUpload] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [FileUpload] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [FileUpload] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [FileUpload] SET  DISABLE_BROKER 
GO
ALTER DATABASE [FileUpload] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [FileUpload] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [FileUpload] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [FileUpload] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [FileUpload] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [FileUpload] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [FileUpload] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [FileUpload] SET RECOVERY FULL 
GO
ALTER DATABASE [FileUpload] SET  MULTI_USER 
GO
ALTER DATABASE [FileUpload] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [FileUpload] SET DB_CHAINING OFF 
GO
ALTER DATABASE [FileUpload] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [FileUpload] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [FileUpload] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [FileUpload] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'FileUpload', N'ON'
GO
ALTER DATABASE [FileUpload] SET QUERY_STORE = OFF
GO
USE [FileUpload]
GO
/****** Object:  UserDefinedTableType [dbo].[InvoiceTransTableType]    Script Date: 5/3/2022 1:33:41 AM ******/
CREATE TYPE [dbo].[InvoiceTransTableType] AS TABLE(
	[TransId] [nvarchar](50) NOT NULL,
	[Amount] [decimal](18, 5) NOT NULL,
	[CurrCode] [varchar](10) NOT NULL,
	[TransDate] [datetime] NOT NULL,
	[Status] [varchar](20) NOT NULL
)
GO
/****** Object:  Table [dbo].[invtrans]    Script Date: 5/3/2022 1:33:41 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[invtrans](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TransId] [nvarchar](50) NULL,
	[Amount] [decimal](18, 5) NULL,
	[CurrCode] [varchar](10) NULL,
	[TransDate] [datetime] NULL,
	[Status] [varchar](20) NULL,
	[CreatedDate] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[spc_invtrans]    Script Date: 5/3/2022 1:33:41 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Aung
-- Create date: 02-May-2022
-- Description:	Create Invoice Transactions
-- =============================================
CREATE PROCEDURE [dbo].[spc_invtrans]
	@TblInvTrans	InvoiceTransTableType readonly,
	@o_succeed		bit = 0 output,
	@o_msgnum		int	output,
	@o_addinfo		nvarchar(500) output
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		insert into invtrans(TransId,Amount,CurrCode,TransDate,Status,CreatedDate)
		select TransId, Amount, CurrCode, TransDate, Status, getdate()
		from @TblInvTrans
		
		set @o_succeed = 1;
		set @o_msgnum = 0;
		set @o_addinfo = '';

	END TRY
	BEGIN CATCH
		DECLARE @ErrorNumber    INT            = ERROR_NUMBER()
		DECLARE @ErrorMessage   NVARCHAR(4000) = ERROR_MESSAGE()
		DECLARE @ErrorProcedure NVARCHAR(4000) = ERROR_PROCEDURE()
		DECLARE @ErrorLine      INT            = ERROR_LINE() 

		set @o_succeed = 0;
		set @o_msgnum = 11;
		set @o_addinfo = 'at [' + OBJECT_NAME(@@PROCID) + '] ' + cast(@ErrorNumber as nvarchar) + '--' + @ErrorMessage;
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[spl_invtrans_by_currency]    Script Date: 5/3/2022 1:33:41 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Aung
-- Create date: 02-May-2022
-- Description:	Select Invoice Transactions By Currency
-- =============================================
CREATE PROCEDURE [dbo].[spl_invtrans_by_currency]
	@CurrCode		varchar(10)
AS
BEGIN
	SET NOCOUNT ON;

	select
		Id
		,TransId
		,Amount
		,CurrCode
		,TransDate
		,Status
		,CreatedDate
	from invtrans
	where CurrCode = @CurrCode
END
GO
/****** Object:  StoredProcedure [dbo].[spl_invtrans_by_status]    Script Date: 5/3/2022 1:33:41 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Aung
-- Create date: 02-May-2022
-- Description:	Select Invoice Transactions By Status
-- =============================================
CREATE PROCEDURE [dbo].[spl_invtrans_by_status]
	@Status varchar(20)
AS
BEGIN
	SET NOCOUNT ON;

	select
		Id
		,TransId
		,Amount
		,CurrCode
		,TransDate
		,Status
		,CreatedDate
	from invtrans
	where Status = @Status
END
GO
/****** Object:  StoredProcedure [dbo].[spl_invtrans_by_transdate]    Script Date: 5/3/2022 1:33:41 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Aung
-- Create date: 02-May-2022
-- Description:	Select Invoice Transactions By Transaction Date
-- =============================================
CREATE PROCEDURE [dbo].[spl_invtrans_by_transdate]
	@FromTransDate		date,
	@ToTransDate		date
AS
BEGIN
	SET NOCOUNT ON;

	select
		Id
		,TransId
		,Amount
		,CurrCode
		,TransDate
		,Status
		,CreatedDate
	from invtrans
	where cast(TransDate as date) >= @FromTransDate and cast(TransDate as date) <= @ToTransDate
END
GO
USE [master]
GO
ALTER DATABASE [FileUpload] SET  READ_WRITE 
GO
