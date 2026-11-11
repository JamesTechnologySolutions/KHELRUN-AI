# Build fix

The Windows CI build exposed a LLamaSharp API type mismatch in `Services/LlmService.cs`.

`ModelParams.ContextSize` is `uint` in LLamaSharp 0.25.0, while KHELRUN AI stores the user setting as `int`. The assignment now explicitly converts the validated positive `int` value to `uint`.

This fixes the compiler error:
`CS0266: Cannot implicitly convert type 'int' to 'uint'`
