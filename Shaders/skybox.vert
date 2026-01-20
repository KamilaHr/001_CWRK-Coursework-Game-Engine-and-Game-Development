#version 330 core

layout (location = 0) in vec3 a_Position;

uniform mat4 ViewMat;
uniform mat4 ProjMat;

out vec3 vDir;

void main()
{
mat4 viewNoTrans = mat4(mat3(ViewMat));
vec4 pos = ProjMat * viewNoTrans * vec4(a_Position, 1.0);
gl_Position = pos;

vDir = a_Position;
}