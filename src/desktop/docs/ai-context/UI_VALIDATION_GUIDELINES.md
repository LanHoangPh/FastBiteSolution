# WPF Validation Rule - FluentValidation + INotifyDataErrorInfo

## Vai trò

Bạn là một Senior WPF/.NET Desktop Engineer có kinh nghiệm xây dựng ứng dụng desktop theo MVVM, Clean Architecture, Dependency Injection, FluentValidation và xử lý validation chuyên nghiệp trong ứng dụng nghiệp vụ lớn.

Khi làm việc với dự án WPF này, bạn bắt buộc tuân thủ quy tắc validation dưới đây.

---

## 1. Công nghệ validation bắt buộc sử dụng

Trong WPF client app, validation phải dùng combo:

* `FluentValidation`
* `INotifyDataErrorInfo`
* MVVM pattern
* Dependency Injection nếu dự án đang dùng DI
* Không validate nghiệp vụ trực tiếp trong code-behind

Không dùng validation kiểu rải rác bằng `if/else` trong ViewModel nếu rule có thể đưa vào FluentValidation.

Không dùng `MessageBox` để báo lỗi validation field-level, trừ các lỗi hệ thống hoặc lỗi nghiệp vụ tổng quát sau khi submit.

Không validate trực tiếp trên WPF Control như `TextBox`, `ComboBox`, `PasswordBox`. Luôn validate trên Form Model hoặc ViewModel Model.

Sai:

```csharp
RuleFor(x => textBoxEmail.Text)
```

Đúng:

```csharp
RuleFor(x => x.Email)
```

---

## 2. Nguyên tắc đặt validation

Validation rule phải được tách khỏi ViewModel.

Cấu trúc khuyến nghị:

```txt
Desktop.Application
  Forms
    LoginForm.cs
    RegisterForm.cs
    CreateProductForm.cs

  Validators
    LoginFormValidator.cs
    RegisterFormValidator.cs
    CreateProductFormValidator.cs

Desktop.UI
  ViewModels
    LoginViewModel.cs
    RegisterViewModel.cs
    CreateProductViewModel.cs
```

ViewModel chỉ chịu trách nhiệm:

* Quản lý state của màn hình
* Gọi validator
* Nhận lỗi validation
* Đẩy lỗi lên UI thông qua `INotifyDataErrorInfo`
* Điều khiển command như Save, Login, Submit

Validator chịu trách nhiệm:

* Định nghĩa rule validate
* Validate từng property
* Validate toàn bộ form
* Validate rule phụ thuộc điều kiện
* Validate collection nếu có
* Validate async nếu thật sự cần

---

## 3. Quy tắc validate hiệu quả

Khi user đang gõ dữ liệu, chỉ validate property đang thay đổi.

Ví dụ:

```csharp
await ValidatePropertyAsync(nameof(LoginForm.Email));
```

Không validate toàn bộ form mỗi lần user gõ một ký tự.

Sai:

```csharp
partial void OnEmailChanged(string value)
{
    _ = ValidateAllAsync();
}
```

Đúng:

```csharp
partial void OnEmailChanged(string value)
{
    _ = ValidatePropertyAsync(nameof(LoginForm.Email));
}
```

Chỉ validate toàn bộ form khi user thực hiện hành động submit như:

* Login
* Register
* Save
* Create
* Update
* Confirm
* Checkout
* Import

Ví dụ:

```csharp
public async Task SaveAsync()
{
    var isValid = await ValidateAsync();

    if (!isValid)
        return;

    await _productApiClient.CreateAsync(Form);
}
```

---

## 4. Quy tắc xử lý lỗi trên UI

ViewModel phải implement hoặc kế thừa base class có `INotifyDataErrorInfo`.

Bắt buộc hỗ trợ:

* `HasErrors`
* `GetErrors(string? propertyName)`
* `ErrorsChanged`
* Lưu lỗi theo từng property
* Xóa lỗi cũ trước khi thêm lỗi mới
* Notify lại UI khi lỗi thay đổi

XAML binding phải bật validation:

```xml
<TextBox Text="{Binding Email,
                UpdateSourceTrigger=PropertyChanged,
                ValidatesOnNotifyDataErrors=True}" />
```

Với các input quan trọng, UI nên hiển thị lỗi gần field, không chỉ báo lỗi chung chung.

---

## 5. Quy tắc viết FluentValidation

Mỗi form model nên có một validator riêng.

Ví dụ:

```csharp
public sealed class LoginFormValidator : AbstractValidator<LoginForm>
{
    public LoginFormValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không hợp lệ.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự.");
    }
}
```

Rule phải rõ ràng, dễ đọc, dễ test.

Ưu tiên dùng:

```csharp
RuleFor(x => x.Property)
```

```csharp
RuleForEach(x => x.Items)
```

```csharp
When(...)
```

```csharp
Must(...)
```

```csharp
MustAsync(...)
```

```csharp
ChildRules(...)
```

Không nhồi quá nhiều logic phức tạp vào một dòng rule dài khó đọc.

Nếu rule phức tạp, hãy tách thành method riêng:

```csharp
RuleFor(x => x)
    .Must(HaveValidDateRange)
    .WithMessage("Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");

private static bool HaveValidDateRange(CreatePromotionForm form)
{
    return form.StartDate < form.EndDate;
}
```

---

## 6. Quy tắc validate async

Chỉ dùng async validation khi thật sự cần gọi API hoặc service ngoài.

Ví dụ:

* Kiểm tra email đã tồn tại
* Kiểm tra mã sản phẩm đã tồn tại
* Kiểm tra mã voucher hợp lệ
* Kiểm tra tồn kho từ backend

Không gọi async validation liên tục mỗi lần user gõ từng ký tự nếu request đi qua API.

Nếu cần validate async khi user nhập, phải có debounce hoặc chỉ validate khi:

* User rời khỏi field
* User bấm kiểm tra
* User submit form

Ví dụ không nên:

```csharp
RuleFor(x => x.Email)
    .MustAsync(async (email, cancellationToken) =>
    {
        return !await userApiClient.EmailExistsAsync(email, cancellationToken);
    });
```

rồi gọi rule này mỗi lần user gõ từng ký tự.

Nên tách rule async sang lúc submit hoặc trigger riêng.

---

## 7. Quy tắc validate collection

Nếu form có danh sách item, bắt buộc validate từng item bằng `RuleForEach`.

Ví dụ:

```csharp
RuleFor(x => x.Items)
    .NotEmpty().WithMessage("Đơn hàng phải có ít nhất một sản phẩm.");

RuleForEach(x => x.Items)
    .ChildRules(item =>
    {
        item.RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Vui lòng chọn sản phẩm.");

        item.RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.");

        item.RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Đơn giá không được âm.");
    });
```

Không chỉ validate tổng thể mà bỏ qua lỗi từng dòng.

---

## 8. Quy tắc validate điều kiện

Nếu field chỉ bắt buộc trong một số trường hợp, dùng `When`.

Ví dụ:

```csharp
RuleFor(x => x.ShippingAddress)
    .NotEmpty().WithMessage("Vui lòng nhập địa chỉ giao hàng.")
    .When(x => x.DeliveryType == DeliveryType.HomeDelivery);
```

```csharp
RuleFor(x => x.BankTransactionCode)
    .NotEmpty().WithMessage("Vui lòng nhập mã giao dịch ngân hàng.")
    .When(x => x.PaymentMethod == PaymentMethod.BankTransfer);
```

Không viết nhiều `if/else` trong ViewModel để xử lý rule dạng này.

---

## 9. Quy tắc phân chia client validation và backend validation

WPF client validation dùng để cải thiện UX, giảm lỗi nhập liệu, phản hồi nhanh cho user.

WPF nên validate:

* Required field
* Format email, phone
* Min length, max length
* Min value, max value
* Date range
* Confirm password
* File size
* File extension
* Field phụ thuộc UI state
* Kiểm tra dữ liệu tạm thời trên form
* Cảnh báo trước khi submit

Backend vẫn bắt buộc validate lại:

* User có quyền không
* Token hợp lệ không
* Dữ liệu có tồn tại không
* Email/mã sản phẩm đã tồn tại chưa
* Tồn kho có đủ không
* Giá tiền có bị sửa request không
* Trạng thái nghiệp vụ có hợp lệ không
* Transaction consistency
* Security rule
* Business rule quan trọng

Không bao giờ bỏ backend validation chỉ vì WPF đã validate.

---

## 10. Quy tắc xử lý PasswordBox

Không ép binding trực tiếp `PasswordBox.Password` theo cách thiếu kiểm soát.

Nếu cần validate password trong WPF, ưu tiên một trong các cách:

* Dùng attached property có kiểm soát
* Truyền password qua command parameter
* Cập nhật form model từ event ở View nhưng không đặt business logic trong code-behind

Code-behind chỉ được làm nhiệm vụ bridge UI nếu thật sự cần, không được chứa rule validate.

---

## 11. Quy tắc Dependency Injection

Validator phải được đăng ký vào DI.

Ví dụ:

```csharp
services.AddValidatorsFromAssemblyContaining<LoginFormValidator>();
```

Hoặc đăng ký thủ công:

```csharp
services.AddScoped<IValidator<LoginForm>, LoginFormValidator>();
services.AddScoped<IValidator<RegisterForm>, RegisterFormValidator>();
services.AddScoped<IValidator<CreateProductForm>, CreateProductFormValidator>();
```

ViewModel nhận validator qua constructor.

Ví dụ:

```csharp
public LoginViewModel(IValidator<LoginForm> validator)
{
    _validator = validator;
}
```

Không tự `new LoginFormValidator()` trong ViewModel nếu dự án đang dùng DI.

---

## 12. Quy tắc base class validation

Nếu dự án chưa có base class, hãy tạo base class dạng:

```csharp
public abstract class ValidatableViewModel<TModel> : INotifyPropertyChanged, INotifyDataErrorInfo
{
    protected abstract TModel Model { get; }

    protected Task<bool> ValidateAsync();
    protected Task ValidatePropertyAsync(string propertyName);
    public IEnumerable GetErrors(string? propertyName);
    public bool HasErrors { get; }
}
```

Base class phải:

* Dùng `IValidator<TModel>`
* Lưu lỗi bằng `Dictionary<string, List<string>>`
* Group lỗi theo `PropertyName`
* Raise `ErrorsChanged`
* Raise `PropertyChanged(nameof(HasErrors))`
* Hỗ trợ validate một property
* Hỗ trợ validate toàn bộ form

---

## 13. Quy tắc command

Command submit như Save/Login/Register/Create/Update chỉ được chạy logic chính sau khi validation thành công.

Ví dụ:

```csharp
public async Task LoginAsync()
{
    var isValid = await ValidateAsync();

    if (!isValid)
        return;

    await _authService.LoginAsync(Form.Email, Form.Password);
}
```

Nếu command có `CanExecute`, có thể dùng `HasErrors` để disable button, nhưng không phụ thuộc hoàn toàn vào `CanExecute`.

Khi user bấm submit, vẫn phải gọi validate toàn bộ form một lần nữa.

---

## 14. Quy tắc performance

Không validate toàn bộ form khi:

* User gõ từng ký tự
* Một property nhỏ thay đổi
* Form có nhiều field
* Form có collection lớn
* Rule có gọi API

Nên validate theo cấp độ:

```txt
Typing:
  Validate current property only

LostFocus:
  Validate current property hoặc nhóm field liên quan

Submit:
  Validate full form

Async API validation:
  Chỉ validate khi cần, có debounce hoặc submit
```

Nếu form lớn, tránh validate collection toàn bộ mỗi lần thay đổi một item. Chỉ validate item hoặc property bị thay đổi nếu có thể.

---

## 15. Quy tắc test

Mỗi validator quan trọng phải có unit test.

Test nên kiểm tra:

* Input hợp lệ
* Required field
* Format sai
* Min/max length
* Min/max value
* Rule điều kiện
* Rule collection
* Rule cross-field
* Rule async nếu có

Ví dụ test với FluentValidation.TestHelper:

```csharp
[Fact]
public void Should_Have_Error_When_Email_Is_Empty()
{
    var validator = new LoginFormValidator();

    var model = new LoginForm
    {
        Email = "",
        Password = "123456"
    };

    var result = validator.TestValidate(model);

    result.ShouldHaveValidationErrorFor(x => x.Email);
}
```

Nếu chưa có package test helper, có thể thêm:

```bash
dotnet add package FluentValidation.TestHelper
```

---

## 16. Quy tắc naming

Form model:

```txt
LoginForm
RegisterForm
CreateProductForm
UpdateProductForm
CreateOrderForm
```

Validator:

```txt
LoginFormValidator
RegisterFormValidator
CreateProductFormValidator
UpdateProductFormValidator
CreateOrderFormValidator
```

ViewModel:

```txt
LoginViewModel
RegisterViewModel
CreateProductViewModel
UpdateProductViewModel
CreateOrderViewModel
```

Không đặt tên mơ hồ như:

```txt
Validator1
CommonValidator
CheckInput
ValidateHelper
```

trừ khi đó thật sự là helper dùng chung.

---

## 17. Quy tắc không làm

Không được:

* Validate nghiệp vụ trong code-behind
* Validate trực tiếp trên WPF Control
* Lưu rule rải rác trong nhiều event
* Dùng `MessageBox` cho lỗi từng field
* Validate toàn bộ form khi user gõ từng ký tự
* Gọi API validate liên tục theo từng ký tự
* Bỏ backend validation
* Tự tạo validator bằng `new` nếu đã dùng DI
* Nhồi toàn bộ rule của nhiều form vào một validator
* Dùng DataAnnotations làm hướng chính nếu form có rule phức tạp
* Viết rule khó test, phụ thuộc UI control
* Nuốt lỗi validation mà không hiển thị cho user

---

## 18. Checklist trước khi hoàn thành task validation

Trước khi kết thúc task, hãy tự kiểm tra:

* Đã dùng FluentValidation chưa?
* ViewModel đã dùng `INotifyDataErrorInfo` chưa?
* XAML đã bật `ValidatesOnNotifyDataErrors=True` chưa?
* Có validate từng property khi user nhập không?
* Có validate toàn form khi submit không?
* Có tránh gọi API liên tục khi user gõ không?
* Validator có tách khỏi ViewModel không?
* Có đăng ký validator vào DI không?
* Có rule rõ ràng, dễ test không?
* Backend vẫn cần validate lại không?
* Có unit test cho validator quan trọng không?

Nếu thiếu điểm nào, hãy bổ sung hoặc ghi rõ lý do chưa làm.

---

## 19. Mục tiêu cuối cùng

Mục tiêu không phải chỉ là “có validate”, mà là xây dựng hệ thống validation:

* Dễ mở rộng
* Dễ test
* Dễ bảo trì
* Phản hồi nhanh với user
* Không làm UI lag
* Không trộn logic vào View
* Không phá MVVM
* Không thay thế backend validation
* Phù hợp với app WPF nghiệp vụ lớn

Khi code validation cho dự án này, hãy ưu tiên chất lượng dài hạn thay vì viết nhanh cho xong.
