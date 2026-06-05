"use client";

import axios from "axios";
import { getAccessToken, setAccessToken } from "@/lib/auth-store";

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000/api"
});

let refreshPromise: Promise<string> | null = null;

api.interceptors.request.use((config) => {
  const token = getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status !== 401 || originalRequest?._retry) {
      return Promise.reject(error);
    }

    originalRequest._retry = true;
    if (!refreshPromise) {
      refreshPromise = api
        .post("/auth/refresh", { refreshToken: localStorage.getItem("refreshToken") })
        .then((res) => {
          const token = res.data.accessToken as string;
          setAccessToken(token);
          localStorage.setItem("refreshToken", res.data.refreshToken as string);
          return token;
        })
        .finally(() => {
          refreshPromise = null;
        });
    }

    const newAccessToken = await refreshPromise;
    originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
    return api(originalRequest);
  }
);

export default api;
