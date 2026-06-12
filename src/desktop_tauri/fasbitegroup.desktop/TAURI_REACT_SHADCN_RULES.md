# TAURI + REACT + SHADCN DESKTOP APPLICATION RULES

Version: 1.0

---

## 1. Mục tiêu

Mục tiêu của dự án là xây dựng ứng dụng desktop chuyên nghiệp bằng:
* Tauri
* React
* TypeScript
* Tailwind CSS
* shadcn/ui
* Rust

Yêu cầu:
* Dễ bảo trì
* Dễ mở rộng
* Dễ test
* Tách biệt rõ UI và Business Logic
* Không phụ thuộc vào component cụ thể
* Có khả năng phát triển lâu dài cho enterprise application

---

## 2. Nguyên tắc kiến trúc

Luôn áp dụng:
```text
UI
↓
Feature Layer
↓
Service Layer
↓
Tauri Command / API
↓
Rust Native Layer
```

Không được:
```text
Component
↓
invoke()
```
trực tiếp ở nhiều nơi.

Mọi tương tác với Tauri Command phải đi qua Service Layer.

---

## 3. Cấu trúc thư mục

Bắt buộc sử dụng Feature-Based Architecture.
```text
src/
│
├── app/
│
├── features/
│   ├── auth/
│   ├── calculator/
│
├── shared/
│   ├── components/
│   ├── hooks/
│   ├── utils/
│   ├── constants/
│
├── services/
│
├── stores/
│
├── styles/
│
└── types/
```

Không được tổ chức toàn bộ dự án bằng `components/`, `pages/`, `utils/` ở cấp root.

---

## 4. Quy tắc đặt tên

### Components
PascalCase
```text
LoginForm.tsx
ProductTable.tsx
UserProfileCard.tsx
```

### Hooks
camelCase
```text
useAuth.ts
useProducts.ts
useTheme.ts
```

### Services
kebab-case
```text
auth-service.ts
product-service.ts
window-service.ts
```

### Stores
kebab-case
```text
auth-store.ts
theme-store.ts
```

### Types
PascalCase
```ts
interface UserDto
interface ProductResponse
type LoginRequest
```

---

## 5. TypeScript Rules

Bắt buộc bật:
```json
{
  "strict": true
}
```

Không sử dụng `any` trừ khi có lý do đặc biệt. Ưu tiên `unknown` sau đó validate.

---

## 6. Validation Rules

Mọi form phải sử dụng:
* React Hook Form
* Zod

Không validate thủ công trong component.

Ví dụ:
```ts
const LoginSchema = z.object({
  email: z.string().email(),
  password: z.string().min(6),
});
```

---

## 7. State Management Rules

Phân chia rõ:
* **Server State**: Sử dụng `TanStack Query`
* **UI State**: Sử dụng `React State` hoặc `Zustand`

Không lưu dữ liệu API trong Zustand.

---

## 8. Service Layer Rules

Mọi API gọi ra ngoài phải đi qua `services/api/` (ví dụ: `auth-api.ts`, `product-api.ts`). Không gọi `fetch()` hay `axios()` trực tiếp trong component.

---

## 9. Tauri Command Rules

Tất cả command phải được bọc.
Ví dụ:
```ts
export async function openFile() {
  return invoke(...)
}
```
Component chỉ được gọi `await openFile()`. Không được `await invoke(...)` trực tiếp trong UI.

---

## 10. Error Handling

Không hiển thị lỗi raw (`500 Internal Server Error`, `thread panicked`, `invoke failed`) cho người dùng. Phải map sang thông báo thân thiện (ví dụ: `Không thể kết nối máy chủ.`, `Phiên đăng nhập đã hết hạn.`).

---

## 11. Logging

Frontend phải có Logger Service. Không để `console.log()` trong production code. Rust sử dụng `tracing` và `tauri-plugin-log`.

---

## 12. Authentication

Không lưu JWT trong localStorage. Ưu tiên:
* Access Token: Memory
* Refresh Token: Secure Storage

Không log token, password, hay secret.

---

## 13. Security

Không sử dụng `dangerouslySetInnerHTML` trừ trường hợp đặc biệt. Mọi dữ liệu người dùng phải được validate. Mọi command native phải kiểm tra đầu vào. Không expose command nguy hiểm cho frontend.

---

## 14. UI Rules

shadcn/ui là nguồn component chuẩn. Không sửa trực tiếp component gốc nếu không cần thiết. Ưu tiên tạo `shared/components/common` để mở rộng (ví dụ: `PageHeader`, `DataTable`, `ConfirmDialog`, `SearchInput`, `LoadingState`, `EmptyState`).

---

## 15. Tailwind Rules

Không lặp lại class dài nhiều lần. Ưu tiên sử dụng `cva()`, `cn()` hoặc reusable component. Không viết 200 ký tự class Tailwind lặp lại ở nhiều file.

---

## 16. Desktop UX Rules

Ứng dụng desktop phải hỗ trợ:
* Loading State
* Error State
* Empty State
* Keyboard Shortcut
* Theme Persistence
* Window State Persistence
* Offline Detection

---

## 17. Rust Architecture

Không đặt business logic trong command. Command chỉ đóng vai trò adapter. Cấu trúc Rust gồm: `commands/`, `services/`, `models/`, `state/`, `errors/`.

---

## 18. Testing

Business Logic phải có khả năng test độc lập. Không nhúng logic vào component. Ưu tiên sử dụng `Vitest` và `React Testing Library`.

---

## 19. Code Review Checklist

Trước khi merge:
* Không có any không cần thiết
* Không gọi API trong component
* Không gọi invoke trực tiếp
* Có xử lý loading
* Có xử lý error
* Có validation
* Có typing đầy đủ
* Không hardcode URL
* Không log dữ liệu nhạy cảm
* Không duplicate code

---

## 20. Nguyên tắc tam phân

Component chỉ chịu trách nhiệm hiển thị. Business Logic thuộc Service Layer. Native Logic thuộc Rust Layer. Không được để UI, Business Logic và Native Logic trộn lẫn với nhau.

---

## 21. Responsive UI & Styling Rules

Ứng dụng phải được thiết kế theo hướng responsive ngay từ đầu.
* **Mobile-first mindset**: Viết style mặc định cho màn hình nhỏ trước, sau đó dùng breakpoint (`sm:`, `md:`, `lg:`, `xl:`).
* **Hạn chế hardcode kích thước**: Dùng `w-full max-w-7xl` thay vì `w-[1200px]`.
* **Chịu được resize**: Tránh tràn chữ, đè nút, modal vượt viewport khi kích thước cửa sổ giảm về `900x600`.
* **Responsive components**: Table hỗ trợ horizontal scroll, sidebar có chế độ collapse, dialog dùng width linh hoạt `w-[calc(100vw-2rem)] max-w-lg`.
* **CSS variables**: Sử dụng CSS variables trong `:root` để định nghĩa theme tokens, không hardcode mã màu.
* **Kiểm tra dark/light mode**: Đảm bảo tất cả component hiển thị tốt trên cả 2 theme.
* **Accessibility**: Giữ nguyên outline mặc định khi focus, cung cấp keyboard navigation và aria-labels cần thiết.
