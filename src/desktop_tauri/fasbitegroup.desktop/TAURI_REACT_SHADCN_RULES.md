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

Mọi form phải có cơ chế validate rõ ràng, tách biệt khỏi logic hiển thị của component. Có thể chọn một trong hai phương án sau:
1. **React Hook Form + Zod**: Thích hợp cho các form lớn phức tạp.
2. **Centralized Pure TS Validators (i18n-ready)**: Đặt tại `src/shared/validation/`. Thích hợp cho các dự án gọn nhẹ, hỗ trợ đa ngôn ngữ. Hàm validate nhận dữ liệu đầu vào và trả về một object lỗi chứa các **i18n translation keys** (ví dụ: `auth.validation.emailRequired`) để hiển thị dịch thuật linh hoạt.

Không viết trực tiếp các đoạn mã kiểm tra chuỗi rỗng (`if (!email)`) hay định dạng regex lặp lại trong hàm submit của component.

Ví dụ Validator bằng Pure TS:
```ts
export const validators = {
  email: (email: string): string | null => {
    if (!email || !email.trim()) return "auth.validation.emailRequired";
    return null;
  }
};
```

Giao diện hiển thị lỗi:
- Trường bị lỗi cần hiển thị viền đỏ (`border-red-500` thông qua hàm `cn()`).
- Nhãn thông báo lỗi dịch thuật `{t(formErrors.fieldName)}` hiển thị trực tiếp bên dưới input bị lỗi.

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
* **Không viết CSS thô (inline styles / local CSS)**: Không được viết inline styles (`style={{...}}`) hoặc tạo các file CSS lẻ cho từng component. Mọi CSS tùy biến phải được định nghĩa trong `globals.css` và tương thích cả giao diện sáng và tối qua biến CSS.
* **Kiểm tra dark/light mode**: Đảm bảo tất cả component hiển thị tốt trên cả 2 theme.
* **Accessibility**: Giữ nguyên outline mặc định khi focus, cung cấp keyboard navigation và aria-labels cần thiết.

---

## 22. Browser Compatibility & Custom Input Behaviors

### 22.1 Tương thích trình duyệt (Browser Dev Mode Compatibility)
Các thư viện hoặc plugin của Tauri như `@tauri-apps/plugin-http` hay `@tauri-apps/api` thường gọi các hàm IPC trực tiếp khi được import. Điều này sẽ làm crash ứng dụng trên trình duyệt web thông thường vì thiếu môi trường Tauri.
- **Quy tắc**: Không import trực tiếp các thư viện này ở top-level của file dùng chung.
- **Giải pháp**: Sử dụng cơ chế lazy-loading thông qua `await import()` bên trong các hàm bất đồng bộ sau khi đã kiểm tra điều kiện chạy môi trường Tauri (`isTauri`).

Ví dụ:
```ts
let tauriFetchCache: any = null;

async function getFetchFn() {
  if (!isTauri) return window.fetch.bind(window);
  if (!tauriFetchCache) {
    const module = await import("@tauri-apps/plugin-http");
    tauriFetchCache = module.fetch;
  }
  return tauriFetchCache;
}
```

### 22.2 Tùy chỉnh Hành vi Nhập liệu (Custom Input Overrides)
Khi tạo các ô nhập liệu tùy biến như Date Picker hoặc Password:
- **Password reveal**: Sử dụng các thuộc tính CSS như `input::-ms-reveal` và `input::-ms-clear` kèm `!important` để ẩn đi nút đổi hiển thị mật khẩu mặc định của Edge/IE, chỉ sử dụng nút tùy biến của UI.
- **Date Picker**: Ẩn icon lịch mặc định của trình duyệt ở bên phải bằng CSS `input[type="date"]::-webkit-calendar-picker-indicator { display: none !important; }`. Sau đó sử dụng React `ref` liên kết với input và kích hoạt trình chọn ngày của hệ thống bằng phương thức `.showPicker()` khi người dùng nhấp vào ô nhập liệu hoặc icon lịch ở đầu dòng.

---

## 23. Shared SVG Icon Wrapper Pattern (Bọc SVG dùng chung)

Để tránh lặp lại các thẻ `<svg>` cồng kềnh trong code React và dễ quản lý:
- **Quy tắc**: Không nhúng trực tiếp thẻ `<svg>` thô trong các component nghiệp vụ.
- **Cách làm**:
  1. Sử dụng component bọc [SvgIcon.tsx](file:///d:/CodeVs/FastBiteSolution/src/desktop_tauri/fasbitegroup.desktop/src/shared/components/ui/svg-icon.tsx) làm wrapper chuẩn.
  2. Tạo component Icon độc lập trong thư mục `src/shared/components/icons/` (ví dụ: `FluentTranslateIcon.tsx`).
  3. Chỉ định chính xác `viewBox` khớp với SVG gốc.
  4. Đặt `fill="currentColor"` và `stroke="none"` (hoặc `strokeWidth={0}`) đối với các icon vẽ bằng mảng khối (`fill-based paths`).

---

## 24. Logging Architecture (Quy tắc Logging trên Frontend)

- **Không dùng console.log trong Production**: Tuyệt đối không để lại các câu lệnh `console.log()` trong code chạy thật.
- **Tauri Native Logger**: Cửa sổ chạy native (Tauri) tích hợp plugin `@tauri-apps/plugin-log` để xuất log trực tiếp ra file `/logs/app.log` đặt tại thư mục gốc của dự án.
- **Logger Wrapper**: Sử dụng logger dùng chung của frontend để tự động chuyển luồng ghi nhật ký linh hoạt giữa console trình duyệt web (Dev mode) và ghi file tauri (Native mode).
