/* START SQL SERVER ONLY SECTION */

use master;
go
if db_id('TestJson') is not null
begin
	alter database TestJson set single_user with rollback immediate;
    drop database TestJson;
end
go
create database TestJson;
go

/* END SQL SERVER ONLY SECTION */

use TestJson;
go
create schema _;
go

create table _.TestJson (
	ProjectName varchar(100),
	ClassName varchar(100),
	MethodName varchar(100),
	TestScenario varchar(100),
	TestCase varchar(100),
	TestFile varchar(100),
	Json varchar(max),
	constraint pk_TestJson
		primary key (ProjectName,ClassName,MethodName,TestScenario,TestCase,TestFile)
);

insert into _.TestJson(ProjectName,ClassName,MethodName,TestScenario,TestCase,TestFile,Json)
	values 
		('EDennis.NetCoreTestingUtilities.Tests', 'ClassA','MethodA',
			'TestScenarioA','TestCaseA','Input','123'),
		('EDennis.NetCoreTestingUtilities.Tests', 'ClassA','MethodA',
			'TestScenarioA','TestCaseA','Expected','["A","B","C"]'),

		('EDennis.NetCoreTestingUtilities.Tests', 'ClassA','MethodA',
			'TestScenarioA','TestCaseB','Input','2018-01-01'),
		('EDennis.NetCoreTestingUtilities.Tests', 'ClassA','MethodA',
			'TestScenarioA','TestCaseB','Expected','["D","E","F"]'),

		('EDennis.NetCoreTestingUtilities.Tests', 'ClassA','MethodA',
			'TestScenarioB','TestCaseA','Input','789'),
		('EDennis.NetCoreTestingUtilities.Tests', 'ClassA','MethodA',
			'TestScenarioB','TestCaseA','Expected','["G","H","I"]'),

		('EDennis.NetCoreTestingUtilities.Tests', 'ClassA','MethodA',
			'TestScenarioB','TestCaseB','Input','abc'),
		('EDennis.NetCoreTestingUtilities.Tests', 'ClassA','MethodA',
			'TestScenarioB','TestCaseB','Expected','["J","K","L"]');
