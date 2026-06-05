"use client";

import { useQuery } from "@tanstack/react-query";
import api from "@/lib/api";

type Product = {
  id: string;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
};

export default function ProductsPage() {
  const query = useQuery({
    queryKey: ["products"],
    queryFn: async () => (await api.get<Product[]>("/products")).data
  });

  if (query.isLoading) return <main className="p-8">Loading products...</main>;
  if (query.isError) return <main className="p-8 text-red-600">Failed to load products.</main>;

  return (
    <main className="mx-auto max-w-6xl p-8">
      <h1 className="mb-6 text-2xl font-bold">Products</h1>
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {query.data?.map((product) => (
          <article key={product.id} className="rounded border bg-white p-4">
            <h2 className="text-lg font-semibold">{product.name}</h2>
            <p className="mt-2 text-sm text-slate-600">{product.description}</p>
            <p className="mt-4 font-semibold">${product.price.toFixed(2)}</p>
            <p className="text-sm text-slate-500">Stock: {product.stockQuantity}</p>
          </article>
        ))}
      </div>
    </main>
  );
}
