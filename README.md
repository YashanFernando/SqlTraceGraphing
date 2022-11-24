# Introduction

This tool will use all the SELECT statements, and stored procedures from your 
SQL profiler output to capture the relationships between tables.
It tries to infer the foreign keys where it's missing.

# How to use

1. Open the SQL profiler and ensure these options are enabled.
   You can have others enabled if you want, we just won't read them.
      1. `SQL:BatchCompleted`
      2. `RPC:Completed`
2. Save the trace as an XML file.
3. Copy the path of the trace file into `Program.cs`.
4. Run the tool.
5. Check the logs to ensure it didn't miss anything.
6. Start the PlantUML Web docker container. </br>
   `docker run -p 8080:8080 plantuml/plantuml-server:jetty`
7. Open the website on `http:\\localhost:8080`
8. Copy the UML diagram. From `@startuml` to `@enduml`.
9. Paste it into the website.

# Quirks

Since we are trying to infer a lot of things, there are a few issues.
It's really trying to infer the best it can and just ignore the rest.

- There's no differentiation between different databases it takes everything from the trace
- It'll include all tables, temp tables and views
- It doesn't remove system tables
- It can't infer links between the outer-query and a sub-query,
  although the links within the sub-query will be captured.