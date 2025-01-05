Delete from [OpenLane-Dev].[dbo].[Bids]
Where ObjectId = '57e3f9d5-a32c-4d9a-94cb-79a3fea2368a'

BEGIN
   IF NOT EXISTS (SELECT * FROM [OpenLane-Dev].[dbo].[Products] 
                   WHERE ObjectId = '37e3f9d5-a32c-4d9a-94cb-79a3fea2368a')
   BEGIN
       INSERT INTO [OpenLane-Dev].[dbo].[Products] ([ObjectId],[Name])
       VALUES ('37e3f9d5-a32c-4d9a-94cb-79a3fea2368a', 'TestProduct')
   END
END

BEGIN
   IF NOT EXISTS (SELECT * FROM [OpenLane-Dev].[dbo].[Offers] 
                   WHERE ObjectId = '47e3f9d5-a32c-4d9a-94cb-79a3fea2368a')
   BEGIN
       INSERT INTO [OpenLane-Dev].[dbo].[Offers] ([ObjectId],[ProductId],[StartingPrice],[OpensAt],[ClosesAt])
       VALUES ('47e3f9d5-a32c-4d9a-94cb-79a3fea2368a', 
			  (SELECT Id FROM [OpenLane-Dev].[dbo].[Products] WHERE ObjectId = '37e3f9d5-a32c-4d9a-94cb-79a3fea2368a'),
			  100,
			  GETDATE(),
			  DATEADD(year, 1, GETDATE()))
   END
END