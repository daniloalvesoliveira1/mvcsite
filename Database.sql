USE [TesteDB]
GO

/****** Object:  Database [TesteDB]    Script Date: 17/08/2022 12:37:34 ******/
CREATE DATABASE [TesteDB]

/****** Object:  Table [dbo].[Status]    Script Date: 17/08/2022 12:37:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Status](
	[id_status] [smallint] NOT NULL,
	[desc_status] [nchar](50) NOT NULL,
 CONSTRAINT [PK_Status] PRIMARY KEY CLUSTERED 
(
	[id_status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Solicitacao](
	[id_solicitacao] [bigint] IDENTITY(1,1) NOT NULL,
	[data_hora_solicitacao] [datetime] NOT NULL,
	[id_status] [smallint] NOT NULL,
	[desc_solicitacao] [nvarchar](100) NULL,
 CONSTRAINT [PK_Solicitacao] PRIMARY KEY CLUSTERED 
(
	[id_solicitacao] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Solicitacao]  WITH CHECK ADD  CONSTRAINT [FK_Solicitacao_Status] FOREIGN KEY([id_status])
REFERENCES [dbo].[Status] ([id_status])
GO

ALTER TABLE [dbo].[Solicitacao] CHECK CONSTRAINT [FK_Solicitacao_Status]
GO

     INSERT INTO Status (id_status,desc_status)
     VALUES (1,'Solicitado');
     INSERT INTO Status (id_status,desc_status)
     VALUES (2,'Processando');
     INSERT INTO Status (id_status,desc_status)
     VALUES (3,'Finalizado');
