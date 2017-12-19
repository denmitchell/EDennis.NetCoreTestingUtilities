declare @Person table (
	ID int,
	FirstName varchar(30),
	LastName varchar(30),
	DateOfBirth datetime
);
declare @Skill table (
	PersonID int,
	Category varchar(30),
	Score int
);

insert into @Person(ID,FirstName,LastName,DateOfBirth)
	values
		(1,'Bob','Jones','1980-01-23'),
		(2,'Jill','Jones','1981-01-24');

insert into @Skill(PersonID,Category,Score)
	values
		(1,'Application Development',3),
		(1,'Project Management',3),
		(2,'Application Development',2),
		(2,'Project Management',1);

declare @j varchar(max);
	set @j =
	(
		select ID,FirstName,LastName,DateOfBirth,
			(select  
				Category,Score
				from @Skill s
				where s.PersonID = p.ID
				for json path) Skills
			from @Person p
			for json path
	);

select @j [json];