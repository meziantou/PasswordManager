import { FormComponent } from "../ui/form-component";
import { required, dataType, DataType } from "../data-annotations";
import { UserService } from "../models/services";
import { Router } from '../router';

export class UserRegisterForm extends FormComponent<UserRegisterModel> {
    constructor(private userService: UserService, private router: Router) {
        super();
    }

    public loadDataCore() {
        return new UserRegisterModel();
    }

    protected async onSubmit(model: UserRegisterModel) {
        try {
            await this.userService.register(model.email, model.password);
            await this.userService.login(model.email, model.password);
            const user = await this.userService.me();

            this.router.setUrl("/user/generate-key");
            return true;
        } catch (ex) {
            console.error(ex);
            return false;
        }
    }
}

class UserRegisterModel {
    @required
    @dataType(DataType.emailAddress)
    email: string = "";

    @required
    @dataType(DataType.password)
    password: string = "";
}