declare @Person table (
	ID int,
	FirstName varchar(30),
	LastName varchar(30),
	DateOfBirth datetime
);

insert into @Person(ID,FirstName,LastName,DateOfBirth)
	values
		(1,'Bob','Jones','1980-01-23'),
		(2,'Jill','Jones','1981-01-24');

select *
	from @Person p
	for json path