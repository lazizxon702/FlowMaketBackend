"use client";

import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import api from "@/lib/api";
import { setAccessToken } from "@/lib/auth-store";
import { useRouter } from "next/navigation";

const schema = z.object({
  email: z.string().email(),
  password: z.string().min(10)
});

type LoginForm = z.infer<typeof schema>;

export default function LoginPage() {
  const router = useRouter();
  const { register, handleSubmit, formState } = useForm<LoginForm>({
    resolver: zodResolver(schema)
  });

  const onSubmit = async (data: LoginForm) => {
    const response = await api.post("/auth/login", data);
    setAccessToken(response.data.accessToken);
    localStorage.setItem("refreshToken", response.data.refreshToken);
    router.push("/products");
  };

  return (
    <main className="mx-auto flex min-h-screen max-w-md items-center p-6">
      <form className="w-full space-y-4 rounded bg-white p-6 shadow" onSubmit={handleSubmit(onSubmit)}>
        <h1 className="text-2xl font-semibold">Sign in</h1>
        <div>
          <label className="mb-1 block text-sm">Email</label>
          <input className="w-full rounded border p-2" {...register("email")} />
          {formState.errors.email && <p className="text-sm text-red-600">{formState.errors.email.message}</p>}
        </div>
        <div>
          <label className="mb-1 block text-sm">Password</label>
          <input type="password" className="w-full rounded border p-2" {...register("password")} />
          {formState.errors.password && <p className="text-sm text-red-600">{formState.errors.password.message}</p>}
        </div>
        <button className="w-full rounded bg-black py-2 text-white" type="submit">
          Login
        </button>
      </form>
    </main>
  );
}
