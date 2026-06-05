import Link from "next/link";

export default function HomePage() {
  return (
    <main className="mx-auto max-w-5xl p-8">
      <h1 className="text-3xl font-bold">FlowMarket</h1>
      <p className="mt-2 text-slate-600">Production-grade marketplace dashboard starter.</p>
      <div className="mt-6 flex gap-3">
        <Link className="rounded bg-black px-4 py-2 text-white" href="/auth/login">
          Login
        </Link>
        <Link className="rounded border border-slate-300 px-4 py-2" href="/products">
          Products
        </Link>
      </div>
    </main>
  );
}
